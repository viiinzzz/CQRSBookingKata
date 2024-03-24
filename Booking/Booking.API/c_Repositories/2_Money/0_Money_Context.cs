namespace CQRSBookingKata.API;

public partial class MoneyRepository(IDbContextFactory factory, ITimeService DateTime) : IMoneyRepository, ITransactionable
{
    private readonly BookingMoneyContext _money = factory.CreateDbContext<BookingMoneyContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _money;
}