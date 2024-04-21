namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestPayment(IClientNotification notification)
    {
        var request = notification.MessageAs<PaymentRequest>();

        var secret = new DebitCardSecrets(request.debitCardOwnerName, request.debitCardExpire, request.debitCardCCV);
        var vendor = new VendorIdentifiers(request.vendorId, request.terminalId);

        using var scope = sp.GetScope<BillingCommandService>(out var billing);

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

            Notify(new ResponseNotification(Omni, PaymentAccepted)
            {
                CorrelationGuid = notification.CorrelationGuid(),
                Message = new { id, invoiceId = request.referenceId }
            });
        }
        catch (Exception e)
        {
            Notify(new ResponseNotification(Omni, PaymentRefused)
            {
                CorrelationGuid = notification.CorrelationGuid(),
                Message = new { invoiceId = request.referenceId }
            });
        }
    }
}