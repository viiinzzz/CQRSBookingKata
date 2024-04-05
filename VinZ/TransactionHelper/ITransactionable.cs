namespace VinZ.Common;

public interface ITransactionable
{
    TransactionContext AsTransaction();
}