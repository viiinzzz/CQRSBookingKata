namespace CQRSBookingKata.Billing;

public class PaymentService
{
    public bool Pay(double amount, string currency, long debitCardNumber, string debitCardOwnerName, int expire,
        int CCV)
    {
        return true;
    }
}