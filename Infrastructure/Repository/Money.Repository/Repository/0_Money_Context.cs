namespace Infrastructure.Storage;

public partial class MoneyRepository(IDbContextFactory factory, ITimeService DateTime) : IMoneyRepository, ITransactionable
{
    private readonly MoneyContext _money = factory.CreateDbContext<MoneyContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _money;
}