
namespace BookingKata.Sales;

public partial class BookingCommandService
(
    ISalesRepository sales,

    IMessageBus bus,

    IGazetteerService geo,
    ITimeService DateTime
)
{
}