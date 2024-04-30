namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestInvoice(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<InvoiceRequest>();

        using var scope = sp.GetScope<IBillingCommandService>(out var billing);

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

        Notify(new ResponseNotification(Omni, InvoiceEmitted, id)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}