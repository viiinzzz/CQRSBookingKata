namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public int EmitReceipt
    (
        long debitCardNumber,
        DebitCardSecrets secret,
        
        int invoiceId,
        long correlationId1,
        long correlationId2
    )
    {
        var now = DateTime.UtcNow;

        var invoice = money.Invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);

        if (invoice == null)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(invoiceId));
        }

        if (invoice.CorrelationId1 != correlationId1 ||
            invoice.CorrelationId2 != correlationId2)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(Correlation));
        }


        var paid = payment.Pay(
            invoice.Amount, 
            invoice.Currency,
            debitCardNumber, 
            secret.ownerName,
            secret.expire,
            secret.CCV
         );

        if (!paid)
        {
            throw new PaymentFailureException();
        }


        var receipt = new Receipt(
            debitCardNumber,
            invoice.Amount, 
            invoice.Currency,
            now, 
            invoice.CustomerId,
            invoiceId,
            correlationId1,
            correlationId2
        );
        
        var receiptId = money.AddReceipt(receipt);

        return receiptId;
    }
}