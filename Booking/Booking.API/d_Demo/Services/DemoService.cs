namespace CQRSBookingKata.API.Demo;

public partial class DemoService
(
    DemoContext demo,
    IAdminRepository admin,
    IMoneyRepository money,
    ISalesRepository sales,

    SalesQueryService sales2,
    BookingCommandService booking,

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

            var context = new TransactionContext() * admin * money * sales;

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

    public int SimulationDay => demo.SimulationDay;

    private const int DayMilliseconds = 24 * 60 * 1000;
    public const double SpeedFactorOneDayOneMinute = 24 * 60;
    public async Task<DateTime> Forward(int days, double? speedFactor, CancellationToken cancellationToken)
    {
        try
        {
            var context = new TransactionContext() * admin * money * sales;

            for (var d = 0; d < days; d++)
            {
                var milliseconds = (int)(DayMilliseconds / (speedFactor ?? SpeedFactorOneDayOneMinute));
                if (milliseconds < 1000) milliseconds = 1000;

                await Task.Delay(milliseconds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                DateTime.Forward(TimeSpan.FromDays(1));
                demo.SimulationDay++;

                context.Execute(() => Fake_BookingDay(false));

                if (days == SeasonDayNumbers)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            var message = @$"
Demo Forward aborted!

ERROR: {ex}";

            Console.Error.WriteLine(message);
        }

        return DateTime.UtcNow;
    }
}