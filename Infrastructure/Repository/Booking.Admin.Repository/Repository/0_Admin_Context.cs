using BookingKata.Shared;

namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository(
    IDbContextFactory factory,
    IGazetteerService geo,
    BookingConfiguration bconf
    // ITimeService DateTime
    ) : IAdminRepository, ITransactionable
{
    private readonly BookingAdminContext _admin = factory.CreateDbContext<BookingAdminContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _admin;
}
