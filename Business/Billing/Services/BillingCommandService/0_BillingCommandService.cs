
namespace BookingKata.Billing;

public partial class BillingCommandService
(
    IMoneyRepository money,

    IPaymentCommandService payment,

    ITimeService DateTime,
    IRandomService rand
)
{

    private const string? Correlation = null;
}
