
namespace BookingKata.Sales;


public partial class SalesQueryService
(
    ISalesRepository sales,

    IMessageBus bus,

    IGazetteerService geo,
    BookingConfiguration bconf,
    ITimeService DateTime
)
{
    private const int FindMinKm = 1;
    private const int FindMaxKm = 200;


    public const int FreeLockMinutes = 30;
}