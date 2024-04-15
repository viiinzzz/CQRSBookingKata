namespace Common.Infrastructure.Bus.Billing;

public partial class BillingBus(IScopeProvider sp) : MessageBusClientBase
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
                    case RequestQuotation:
                    {
                        Verb_Is_RequestQuotation(notification, sp);
                        break;
                    }
                    case RequestInvoice:
                    {
                        Verb_Is_RequestInvoice(notification, sp);
                        break;
                    }
                    case RequestPayment:
                    {
                        Verb_Is_RequestPayment(notification, sp);
                        break;
                    }
                    case RequestRefund:
                    {
                        Verb_Is_RequestRefund(notification, sp);
                        break;
                    }
                    case RequestPage:
                    {
                        Verb_Is_RequestPage(notification, sp);
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