namespace BookingKata.ThirdParty;

public interface IPaymentCommandService
{
    PaymentRequestResponse RequestReceipt
    (
        double amount,
        string currency,
        
        long debitCardNumber,
        string debitCardOwnerName,
        int expire,
        int CCV,

        int vendorId,
        int terminalId,
        int transactionId
    );
}