namespace BookingKata.Billing;

public interface IBillingCommandService
{
    Id EmitQuotation
    (
        double price,
        string currency,
        DateTime optionStartUtc,
        DateTime optionEndUtc,
        string jsonMeta,

        int referenceId,
        long correlationId1,
        long correlationId2
    );

    int EmitRefund
    (
        int receiptId,
        long correlationId1,
        long correlationId2
    );

    Id EmitReceipt
    (
        double amount,
        string currency,

        long debitCardNumber,
        DebitCardSecrets secrets,
        VendorIdentifiers vendor,

        int invoiceId,
        long correlationId1,
        long correlationId2
    );

    Id EmitInvoice
    (
        double amount,
        string currency,

        int customerId,
        int quotationId,
        long correlationId1,
        long correlationId2
    );
}