namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestRefund(IClientNotification notification)
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

        Notify(new ResponseNotification(Omni, RefundEmitted)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = new { id }
        });
    }
}