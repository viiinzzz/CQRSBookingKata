
namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public int EmitReceipt
    (
        double amount,
        string currency,

        long debitCardNumber,
        DebitCardSecrets secrets,
        VendorIdentifiers vendor,
        
        int invoiceId,
        long correlationId1,
        long correlationId2
    )
    {
        var now = DateTime.UtcNow;

        var invoice = money.Invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);

        if (invoice == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(invoiceId));
        }

        if (invoice.CorrelationId1 != correlationId1 ||
            invoice.CorrelationId2 != correlationId2)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(Correlation));
        }

        var originator = GetType().FullName 
                         ?? throw new ArgumentException("invalid originator");

        //
        //
        var paid = bus.AskResult<PaymentResponse>(
            originator, Common.Services.ThirdParty.Recipient, Common.Services.ThirdParty.Verb.RequestPayment,
            new PaymentOrder
            {
                amount = invoice.Amount,
                currency = invoice.Currency,

                debitCardNumber = debitCardNumber,
                debitCardOwnerName = secrets.ownerName,
                expire = secrets.expire,
                CCV = secrets.CCV,

                vendorId = vendor.vendorId,
                terminalId = vendor.terminalId,
                transactionId = invoiceId
            });
        //
        //

        if (paid is not { Accepted: true })
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