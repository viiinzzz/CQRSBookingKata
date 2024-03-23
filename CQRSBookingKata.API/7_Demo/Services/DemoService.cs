namespace CQRSBookingKata.API.Demo;

public partial class DemoService
(
    IAdminRepository _admin,
    IMoneyRepository _money,
    ISalesRepository _sales,

    SalesQueryService _sales2,
    BookingCommandService _booking,

    ITimeService DateTime
)
{
    private const int StaffPerHotel = 3;
    private const int ManagerPerHotel = 1;
    private const int HotelCount = 3;
    private const int FloorPerHotel = 2;
    private const int RoomPerFloor = 3;
    private const int PersonPerRoom = 2;

    private const int SeasonDayNumbers = 250;
    private const int CustomerCount = 1000;


    public async Task Execute(CancellationToken cancel)
    {
        try
        {
            DateTime.Freeze();

            var context = new TransactionContext() * _admin * _money * _sales;

            context.Execute(() => Fake_Employees(false));
            context.Execute(() => Fake_Hotels(false));
            context.Execute(() => Fake_Vacancies(false));
            context.Execute(() => Fake_Customers(false));


            // DateTime.Unfreeze();


            cancel.ThrowIfCancellationRequested();
        }
        catch (Exception ex)
        {
            var message = @$"
Demo Seed aborted!

ERROR: {ex}";

            Console.Error.WriteLine(message);
        }
    }

    public async Task Forward(int days, CancellationToken cancel)
    {
        try
        {
            DateTime.Freeze();

            var context = new TransactionContext() * _admin * _money * _sales;

            for (var d = 0; d < /*SeasonDayNumbers*/days; d++)
            {
                context.Execute(() => Fake_BookingDay(false));
            }

            DateTime.Unfreeze();


            cancel.ThrowIfCancellationRequested();
        }
        catch (Exception ex)
        {
            var message = @$"
Demo Forward aborted!

ERROR: {ex}";

            Console.Error.WriteLine(message);
        }
    }
}