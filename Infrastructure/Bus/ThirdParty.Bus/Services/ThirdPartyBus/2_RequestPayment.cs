namespace Support.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPayment(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PaymentRequest>();


        using var scope = sp.GetScope<IPaymentCommandService>(out var payment);


        //
        //
        var response = payment.RequestPayment
        (
            request.referenceId,

            request.amount,
            request.currency,

            request.debitCardNumber,
            request.debitCardOwnerName,
            request.debitCardExpire,
            request.debitCardCCV,

            request.vendorId,
            request.terminalId
        );
        //
        //

        var responseVerb = response.Accepted ? PaymentAccepted : PaymentRefused;

        Notify(new ResponseNotification(Omni, responseVerb, response)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2,
        });
    }
}