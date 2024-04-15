namespace Common.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestRefund(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<RefundRequest>();

        using var scope = sp.GetScope<BillingCommandService>(out var billing);

        //
        //
        var id = billing.EmitRefund
        (
            request.receiptId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new Notification(Omni, QuotationEmitted)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = new { id }
        });
    }
}