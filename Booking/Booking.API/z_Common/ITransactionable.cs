namespace BookingKata.API.Helpers;

public interface ITransactionable
{
    TransactionContext AsTransaction();
}