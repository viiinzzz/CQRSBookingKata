namespace CQRSBookingKata.Billing;

public class PaymentCommandService
{
    public bool Pay(double amount, string currency, long debitCardNumber, string debitCardOwnerName, int expire,
        int CCV)
    {
        return true;
    }
}