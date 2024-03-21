

namespace CQRSBookingKata.API.Demo;

public class DemoService : IHostedLifecycleService
{
    private readonly IAdminRepository _admin;
    private readonly IMoneyRepository _money;
    // private readonly ISalesRepository _sales;
    private readonly BookingCommandService _booking;

    private readonly ITimeService DateTime;

    public DemoService(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();

        _admin = scope.ServiceProvider.GetRequiredService<IAdminRepository>();
        _money = scope.ServiceProvider.GetRequiredService<IMoneyRepository>();
        
        _booking = scope.ServiceProvider.GetRequiredService<BookingCommandService>();

        DateTime = scope.ServiceProvider.GetRequiredService<ITimeService>();
    }

    private Task _executeTask = Task.CompletedTask;
    private CancellationTokenSource _executeCancel = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _executeTask = Execute(_executeCancel.Token);
    }

    public async Task StartingAsync(CancellationToken cancellationToken) {}

    public async Task StartedAsync(CancellationToken cancellationToken) { }

    protected async Task Execute(CancellationToken cancel)
    {
        try
        {
            DateTime.Freeze();

            var context = new TransactionContext() * _admin * _money;
            
            context.Execute(() =>
            {
                Fake_Employees_Hotels_Vacancies(false);
            });
            
            DateTime.Unfreeze();


            cancel.ThrowIfCancellationRequested();
        }
        catch (Exception ex)
        {
            var message = @$"
Demo aborted!

ERROR: {ex}";

            Console.Error.WriteLine(message);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _executeCancel.Cancel();

        await Task.WhenAny(_executeTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public async Task StoppingAsync(CancellationToken cancellationToken) { }

    public async Task StoppedAsync(CancellationToken cancellationToken) { }






    private const int StaffPerHotel = 3;
    private const int ManagerPerHotel = 1;
    private const int HotelCount = 3;
    private const int FloorPerHotel = 2;
    private const int RoomPerFloor = 3;
    private const int PersonPerRoom = 2;

    private const int SeasonDayNumbers = 250;

    private void Fake_Employees_Hotels_Vacancies(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var fakeStaffIds = RandomHelper
                .GenerateFakeEmployees(HotelCount * StaffPerHotel)
                .Select(fake =>
                {
                    var employeeId = _admin.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                    var payrollId = _money.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                    return employeeId;
                })
                .ToArray();
            
            var fakeManagerIds = RandomHelper
                .GenerateFakeEmployees(HotelCount * ManagerPerHotel)
                .Select(fake =>
                {
                    var employeeId = _admin.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                    var payrollId = _money.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                    return employeeId;
                })
                .ToArray();

            var fakeHotelsIds = RandomHelper
                .GenerateFakeHotels(3)
                .Select((fake, hotelNum) =>
                {
                    var managerId = fakeManagerIds[hotelNum];
                    var hotelId = _admin.Create(new NewHotel(fake.HotelName, fake.Latitude, fake.Longitude));
                    _admin.Update(hotelId, new UpdateHotel
                    {
                        EarliestCheckInTime = 16_00,
                        LatestCheckOutTime = 10_00,
                        LocationAddress = fake.LocationAddress,
                        ReceptionPhoneNumber = fake.ReceptionTelephoneNumber,
                        Url = fake.Url,
                        Ranking = fake.ranking,
                        ManagerId = managerId,
                    }, scoped: false);


                    for (var floorNum = 0; floorNum < FloorPerHotel; floorNum++)
                    {
                        _admin.Create(new NewRooms(hotelId, floorNum, RoomPerFloor, PersonPerRoom), scoped: false);
                    }


                    _booking.OpenHotelSeason(
                        hotelId, default, 
                        DateTime.UtcNow, DateTime.UtcNow.AddDays(SeasonDayNumbers), scoped: false);

                    return hotelId;
                })
                .ToArray();


            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(FakeEmployeesAndHotels)}' failed", e);
        }
    }

}
