using BookingKata.ThirdParty;

namespace Common.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPayment(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<PaymentRequest>();


        using var scope = sp.GetScope<IPaymentCommandService>(out var payment);

        //
        //
        var response = payment.RequestReceipt
        (
            request.amount,
            request.currency,

            request.debitCardNumber,
            request.debitCardOwnerName,
            request.debitCardExpire,
            request.debitCardCCV,

            request.vendorId,
            request.terminalId,
            request.transactionId
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