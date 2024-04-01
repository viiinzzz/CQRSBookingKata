namespace VinZ.DbContextHelper;

public interface ITransactionable
{
    TransactionContext AsTransaction();
}