namespace BookingKata.ThirdParty;

public class PaymentCommandService
(
    ITimeService DateTime
) 
    : IPaymentCommandService
{
    public PaymentResponse RequestPayment
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
    )
    {
        //TODO

        Console.WriteLine("FAKE PAYMENT!!!");

        var transactionTime = DateTime.UtcNow;

        var transactionId =
        (
            referenceId,
        
            amount,
            currency,

            vendorId,
            terminalId,

            transactionTime
        ).GetHashCode();

        return new PaymentResponse
        {
            Accepted = true,
            TransactionTimeUtc = transactionTime.SerializeUniversal(),
            TransactionId = transactionId
        };
    }
}