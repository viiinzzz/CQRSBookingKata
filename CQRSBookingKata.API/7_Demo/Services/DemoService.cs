

namespace CQRSBookingKata.API.Demo;

public class DemoService : IHostedLifecycleService
{
    private readonly IAssetsRepository assets;
    private readonly IBillingRepository billing;
    private readonly ITimeService DateTime;

    public DemoService(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();

        assets = scope.ServiceProvider.GetRequiredService<IAssetsRepository>();
        billing = scope.ServiceProvider.GetRequiredService<IBillingRepository>();
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
        DateTime.Freeze();

        FakeEmployeesAndHotels();

        DateTime.Unfreeze();


        cancel.ThrowIfCancellationRequested();

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

    private void FakeEmployeesAndHotels()
    {
        var fakeStaffIds = RandomHelper
            .GenerateFakeEmployees(HotelCount * StaffPerHotel)
            .Select(fake =>
            {
                var employeeId = assets.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                var payrollId = billing.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                return employeeId;
            })
            .ToArray();
        
        var fakeManagerIds = RandomHelper
            .GenerateFakeEmployees(HotelCount * ManagerPerHotel)
            .Select(fake =>
            {
                var employeeId = assets.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                var payrollId = billing.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                return employeeId;
            })
            .ToArray();

        var fakeHotelsIds = RandomHelper
            .GenerateFakeHotels(3)
            .Select((fake, hotelNum) =>
            {
                var managerId = fakeManagerIds[hotelNum];
                var hotelId = assets.Create(new NewHotel(fake.HotelName, fake.Latitude, fake.Longitude));
                assets.Update(hotelId, new UpdateHotel
                {
                    EarliestCheckInTime = 16_00,
                    LatestCheckOutTime = 10_00,
                    LocationAddress = fake.LocationAddress,
                    ReceptionPhoneNumber = fake.ReceptionTelephoneNumber,
                    Url = fake.Url,
                    Ranking = 2,
                    ManagerId = managerId,
                });


                for (int floorNum = 0; floorNum < FloorPerHotel; floorNum++)
                {
                    assets.Create(new NewRooms(hotelId, floorNum, RoomPerFloor, PersonPerRoom));
                }

                return hotelId;
            })
            .ToArray();


    }

}
