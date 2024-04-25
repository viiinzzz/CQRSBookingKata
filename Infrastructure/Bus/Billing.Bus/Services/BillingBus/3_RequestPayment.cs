namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestPayment(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PaymentRequest>();

        var secret = new DebitCardSecrets(request.debitCardOwnerName, request.debitCardExpire, request.debitCardCCV);
        var vendor = new VendorIdentifiers(request.vendorId, request.terminalId);

        using var scope = sp.GetScope<BillingCommandService>(out var billing);

        var referenceId = new
        {
            invoiceId = request.referenceId
        };

        try
        {
            //
            //
            var id = billing.EmitReceipt
            (
                request.amount,
                request.currency,

                request.debitCardNumber,
                secret,
                vendor,
                request.referenceId,

                notification.CorrelationId1,
                notification.CorrelationId2
            );
            //
            //

            var idAndReferenceId = referenceId.PatchRelax(id);

            Notify(new ResponseNotification(Omni, PaymentAccepted, idAndReferenceId)
            {
                CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
            });
        }
        catch (Exception e)
        {
            Notify(new ResponseNotification(Omni, PaymentRefused, referenceId)
            {
                CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
            });
        }
    }
}