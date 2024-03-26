namespace BookingKata.Sales;

public interface IPaymentCommandService
{
    bool Pay(double amount, string currency, long debitCardNumber, string debitCardOwnerName, int expire,
        int CCV);
}

public class PaymentCommandService : IPaymentCommandService
{
    public bool Pay(double amount, string currency, long debitCardNumber, string debitCardOwnerName, int expire,
        int CCV)
    {
        return true;
    }
}