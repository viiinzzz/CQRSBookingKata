using BookingKata.Infrastructure.Bus.Sales;
using BookingKata.Infrastructure.Network;

namespace BookingKata.Infrastructure;

public static class EnterpriseNetwork
{
    static EnterpriseNetwork()
    {
        _ = nameof(BillingBus);
        _ = nameof(AdminBus);
        _ = nameof(PlanningBus);
        _ = nameof(SalesBus);
    }
}
