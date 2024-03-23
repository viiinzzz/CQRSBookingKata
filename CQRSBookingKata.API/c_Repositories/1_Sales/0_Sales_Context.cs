
namespace CQRSBookingKata.API;

public partial class SalesRepository(IDbContextFactory factory, ITimeService DateTime) : ISalesRepository, ITransactionable
{
    private readonly BookingSalesContext _sales = factory.CreateDbContext<BookingSalesContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _sales;
}