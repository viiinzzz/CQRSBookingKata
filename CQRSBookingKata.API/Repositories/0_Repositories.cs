namespace CQRSBookingKata.API.Repositories;

public partial class AssetsRepository(IDbContextFactory factory, TimeService DateTime) : IAssetsRepository
{
    private readonly BookingBackContext back = factory.CreateDbContext<BookingBackContext>();
}


public partial class BillingRepository(IDbContextFactory factory, TimeService DateTime) : IBillingRepository
{
    private readonly BookingBackContext back = factory.CreateDbContext<BookingBackContext>();
    private readonly BookingSensitiveContext sensitive = factory.CreateDbContext<BookingSensitiveContext>();
}


public partial class SalesRepository(IDbContextFactory factory, TimeService DateTime) : ISalesRepository
{
    private readonly BookingFrontContext front = factory.CreateDbContext<BookingFrontContext>();
}


public partial class PlanningRepository(IDbContextFactory factory, TimeService DateTime) : IPlanningRepository
{
    private readonly BookingFrontContext front = factory.CreateDbContext<BookingFrontContext>();
}