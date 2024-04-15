
using VinZ.MessageQueue;

namespace BookingKata.Billing;

public partial class BillingCommandService
(
    IMoneyRepository money,

    IMessageBus bus,

    ITimeService DateTime
)
{

    private const string? Correlation = null;
}
