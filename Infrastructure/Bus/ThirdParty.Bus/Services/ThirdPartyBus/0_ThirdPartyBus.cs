namespace Support.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {
                    case RequestPayment:
                    {
                        Verb_Is_RequestPayment(notification);
                        break;
                    }

                    case RequestPricing:
                    {
                        Verb_Is_RequestPricing(notification);
                        break;
                    }

                    case RequestPage:
                    {
                        Verb_Is_RequestPage(notification);
                        break;
                    }

                    default:
                    {
                        throw new VerbInvalidException(notification.Verb);
                    }
                }
            }
            catch (Exception ex)
            {
                Notify(new Notification(notification.Originator, ErrorProcessingRequest)
                {
                    CorrelationGuid = notification.CorrelationGuid(),
                    Message = new
                    {
                        message = notification.Message,
                        messageType = notification.MessageType,
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        };
    }
}