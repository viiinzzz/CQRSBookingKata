
namespace BookingKata.Infrastructure;

public static class EnterpriseNetwork
{
    static EnterpriseNetwork()
    {
        _ = nameof(ThirdPartyBus);
        _ = nameof(BillingBus);

        _ = nameof(AdminBus);
        _ = nameof(PlanningBus);
        _ = nameof(SalesBus);
    }
}
