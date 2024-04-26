namespace BookingKata.API.Demo;

public partial class DemoService
(
    BookingDemoContext demo,
    IAdminRepository admin,
    IMoneyRepository money,
    ISalesRepository sales,
    IGazetteerService geo,

    SalesQueryService sales2,
    BookingCommandService booking,

    IMessageBus bus,
    ITimeService DateTime
)
    : MessageBusClientBase
{
    private const int StaffPerHotel = 1;//3;
    private const int ManagerPerHotel = 1;
    private const int HotelCount = 1;//3;
    private const int FloorPerHotel = 1;//2;
    private const int RoomPerFloor = 3;
    private const int PersonPerRoom = 2;

    private const int SeasonDayNumbers = 50;//250
    private const int CustomerCount = 1000;

    private const string originator = nameof(demo);

    public async Task Execute(CancellationToken cancel)
    {
        try
        {
            DateTime.Freeze();

            var context = new TransactionContext() * admin * money * sales * geo;

            //admin setup
            context.Execute(() => Fake_Employees(false));
            context.Execute(() => Fake_Hotels(false));
            context.Execute(() => Fake_Vacancies(false));

            //sales setup
            context.Execute(() => Fake_Customers(false));


            // DateTime.Unfreeze();


            cancel.ThrowIfCancellationRequested();
        }
        catch (Exception ex)
        {
            var message = @$"
Demo Seed aborted!

ERROR: {ex}";

            bus.Notify(new AdvertisementNotification(message, [])
            {
                Originator = originator,
                Immediate = true
            });
        }
    }

    public int SimulationDay => demo.SimulationDay;

    private const int DayMilliseconds = 24 * 60 * 1000;
    public const double SpeedFactorOneDayOneMinute = 24 * 60;
    public async Task<DateTime> Forward(int days, double? speedFactor, CancellationToken cancellationToken)
    {
        try
        {
            var context = new TransactionContext() * admin * money * sales * geo;

            for (var d = 0; d < days; d++)
            {
                var milliseconds = (int)(DayMilliseconds / (speedFactor ?? SpeedFactorOneDayOneMinute));
                if (milliseconds < 1000) milliseconds = 1000;

                await Task.Delay(milliseconds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                DateTime.Forward(TimeSpan.FromDays(1));
                demo.SimulationDay++;

                context.Execute(() => Fake_BookingDay());

                if (days == SeasonDayNumbers)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            var message = @"
Demo Forward aborted!

ERROR: {ex}";

            var args = new object[] { ex.ToString() };

            bus.Notify(new AdvertisementNotification(message, args)
            {
                Originator = originator,
                Immediate = true
            });
        }

        return DateTime.UtcNow;
    }
}