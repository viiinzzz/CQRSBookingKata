namespace BookingKata.ThirdParty;

public interface IPaymentCommandService
{
    PaymentResponse RequestPayment
    (
        int referenceId,

        double amount,
        string currency,
        
        long debitCardNumber,
        string debitCardOwnerName,
        int expire,
        int CCV,

        int vendorId,
        int terminalId
    );
}