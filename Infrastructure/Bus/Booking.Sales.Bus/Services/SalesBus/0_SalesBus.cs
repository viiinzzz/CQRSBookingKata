﻿namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus(IScopeProvider sp, BookingConfiguration bconf) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Sales);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {
                    case Verb.Sales.RequestOpenHotelSeason:
                    {
                        Verb_Is_RequestOpenHotelSeason(notification);
                        break;
                    }
                    case Verb.Sales.RequestBook:
                    {
                        Verb_Is_RequestBook(notification);
                        break;
                    }
                    case Verb.Sales.RequestKpi:
                    {
                        Verb_Is_RequestKpi(notification);
                        break;
                    }
                    case RequestPage:
                    {
                        Verb_Is_RequestPage(notification);
                        break;
                    }

                    case Verb.Sales.RequestStay:
                    {
                        Verb_Is_RequestStay(notification);
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