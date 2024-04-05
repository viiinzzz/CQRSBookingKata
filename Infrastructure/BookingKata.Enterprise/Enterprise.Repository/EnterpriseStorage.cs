using BookingKata.Infrastructure.Storage;
using Infrastructure.Storage;

namespace BookingKata.Infrastructure;

public static class EnterpriseStorage
{
    static EnterpriseStorage()
    {
        _ = nameof(AdminRepository);
        _ = nameof(PlanningRepository);
        _ = nameof(SalesRepository);
        _ = nameof(MessageQueueRepository);
        _ = nameof(GazetteerService);
        _ = nameof(MoneyRepository);
    }
}
