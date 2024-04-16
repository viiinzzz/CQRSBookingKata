namespace Common.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestInvoice(IClientNotification notification)
    {
        var request = notification.MessageAs<InvoiceRequest>();

        using var scope = sp.GetScope<BillingCommandService>(out var billing);

        //
        //
        var id = billing.EmitInvoice
        (
            request.amount,
            request.currency,
            request.customerId,
            request.quotationId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new Notification(Omni, InvoiceEmitted)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = new { id }
        });
    }
}