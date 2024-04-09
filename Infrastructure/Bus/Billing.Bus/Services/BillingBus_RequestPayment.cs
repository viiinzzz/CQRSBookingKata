namespace Common.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestPayment(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<PaymentRequest>();

        var secret = new DebitCardSecrets(request.ownerName, request.expire, request.CCV);

        using var scope = sp.GetScope<BillingCommandService>(out var billing);

        //
        //
        var id = billing.EmitReceipt
        (
            request.debitCardNumber,
            secret,
            request.invoiceId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new NotifyMessage(Omni, QuotationEmitted)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = new { id }
        });
    }
}