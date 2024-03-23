using Microsoft.EntityFrameworkCore.Storage;

namespace CQRSBookingKata.API;

public partial class AdminRepository(IDbContextFactory factory, ITimeService DateTime) : IAdminRepository, ITransactionable
{
    private readonly BookingAdminContext _admin = factory.CreateDbContext<BookingAdminContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _admin;
}
