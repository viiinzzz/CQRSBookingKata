namespace BookingKata.API;

public partial class AdminRepository(
    IDbContextFactory factory,
    IGazeteerService geo
    // ITimeService DateTime
    ) : IAdminRepository, ITransactionable
{
    private readonly BookingAdminContext _admin = factory.CreateDbContext<BookingAdminContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _admin;
}
