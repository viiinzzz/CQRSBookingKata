namespace BookingKata.ThirdParty;

public class PaymentCommandService : IPaymentCommandService
{
    public bool Pay(double amount, string currency, long debitCardNumber, string debitCardOwnerName, int expire,
        int CCV)
    {
        return true;
    }
}