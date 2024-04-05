namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public int EmitRefund
    (
        int receiptId,
        long correlationId1,
        long correlationId2
    )
    {
        var now = DateTime.UtcNow;

        var receipt = money.Receipts.FirstOrDefault(r => r.ReceiptId == receiptId);

        if (receipt == null)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(receiptId));
        }

        if (receipt.CorrelationId1 != correlationId1 ||
            receipt.CorrelationId2 != correlationId2)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(Correlation));
        }


        var refund = new Refund(
            receipt.DebitCardNumber,
            receipt.Amount,
            receipt.Currency,
            now, 
            receipt.CustomerId,
            receipt.InvoiceId,
            receipt.CorrelationId1,
            receipt.CorrelationId2
         );

        var refundId = money.AddRefund(refund);

        return refundId;
    }
}