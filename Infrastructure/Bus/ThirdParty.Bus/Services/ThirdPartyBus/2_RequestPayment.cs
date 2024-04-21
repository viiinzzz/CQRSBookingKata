using Support.Infrastructure.Network;

namespace Support.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPayment(IClientNotification notification)
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

        switch (response.Accepted)
        {
            case true:
            {
                Notify(new Notification(Omni, PaymentAccepted)
                {
                    CorrelationGuid = notification.CorrelationGuid(),
                    Message = response
                });

                break;
            }

            case false:
            {
                Notify(new Notification(Omni, PaymentRefused)
                {
                    CorrelationGuid = notification.CorrelationGuid(),
                    Message = response
                });

                break;
            }
        }
    }
}