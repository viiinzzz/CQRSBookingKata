namespace BookingKata.Billing;

public partial class BillingCommandService
(
    IMoneyRepository money,

    IMessageBus bus,

    ITimeService DateTime
) 
    : IBillingCommandService
{

    private const string? Correlation = null;
}
