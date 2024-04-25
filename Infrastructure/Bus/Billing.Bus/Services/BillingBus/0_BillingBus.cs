﻿namespace Support.Infrastructure.Bus.Billing;

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
                        Verb_Is_RequestQuotation(notification);
                        break;
                    }
                    case RequestInvoice:
                    {
                        Verb_Is_RequestInvoice(notification);
                        break;
                    }
                    case RequestPayment:
                    {
                        Verb_Is_RequestPayment(notification);
                        break;
                    }
                    case RequestReceipt:
                    {
                        Verb_Is_RequestReceipt(notification);
                        break;
                    }
                    case RequestRefund:
                    {
                        Verb_Is_RequestRefund(notification);
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
                var error = new
                {
                    message = notification.Message,
                    messageType = notification.MessageType,

                    error = ex.Message,
                    stackTrace = ex.StackTrace
                };

                Notify(new NegativeResponseNotification(error)
                {
                    Originator = notification.Originator,
                    CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2,
                });
            }
        };
    }
}