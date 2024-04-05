namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository(
    IDbContextFactory factory,
    IGazetteerService geo
    // ITimeService DateTime
    ) : IAdminRepository, ITransactionable
{
    private readonly BookingAdminContext _admin = factory.CreateDbContext<BookingAdminContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _admin;
}
