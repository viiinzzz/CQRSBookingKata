namespace BookingKata.ThirdParty;

public interface IPaymentCommandService
{
    bool Pay(double amount, string currency, long debitCardNumber, string debitCardOwnerName, int expire,
        int CCV);
}