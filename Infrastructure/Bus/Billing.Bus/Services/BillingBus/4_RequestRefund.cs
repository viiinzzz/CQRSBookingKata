namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestRefund(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<RefundRequest>();

        using var scope = sp.GetScope<IBillingCommandService>(out var billing);

        //
        //
        var refundId = billing.EmitRefund
        (
            request.receiptId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new ResponseNotification(Omni, RefundEmitted, refundId)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}