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
            await Task.Delay(30000);


            DateTime.Freeze();

            Seed();

            cancel.ThrowIfCancellationRequested();
        }
        catch (Exception ex)
        {
            var childNotification = new RequestNotification(nameof(Demo), nameof(Seed));

            bus.Notify(new NegativeResponseNotification(childNotification, ex, "aborted!")
            {
                Originator = originator,
                Immediate = true
            });
        }
    }

}