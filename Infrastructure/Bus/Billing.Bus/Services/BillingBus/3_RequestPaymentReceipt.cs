using VinZ.Common;

namespace Common.Infrastructure.Bus.Billing;

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
                request.debitCardNumber,
                secret,
                vendor,
                request.invoiceId,
                notification.CorrelationId1,
                notification.CorrelationId2
            );
            //
            //

            Notify(new Notification(Omni, PaymentAccepted)
            {
                CorrelationGuid = notification.CorrelationGuid(),
                Message = new { id, request.invoiceId }
            });
        }
        catch (Exception e)
        {
            Notify(new Notification(Omni, PaymentRefused)
            {
                CorrelationGuid = notification.CorrelationGuid(),
                Message = new { request.invoiceId }
            });
        }
    }
}