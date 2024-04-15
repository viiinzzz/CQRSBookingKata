using Common.Infrastructure.Network;
using VinZ.MessageQueue;

namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public int EmitReceipt
    (
        long debitCardNumber,
        DebitCardSecrets secrets,
        
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
        var paid = bus.AskResult<PaymentRequestResponse>(
            originator, Common.Services.ThirdParty.Recipient, Common.Services.ThirdParty.Verb.RequestPayment,
            new Common.Infrastructure.Network.PaymentOrder(debitCardNumber, secrets.ownerName, secrets.expire, secrets.CCV, invoiceId));
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