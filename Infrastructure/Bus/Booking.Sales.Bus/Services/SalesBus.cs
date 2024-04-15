namespace BookingKata.Infrastructure.Bus.Sales;

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
                        Verb_Is_RequestOpenHotelSeason(notification, sp);
                        break;
                    }
                    case Verb.Sales.RequestBook:
                    {
                        Verb_Is_RequestBook(notification, sp);
                        break;
                    }
                    case Verb.Sales.RequestKpi:
                    {
                        Verb_Is_RequestKpi(notification, sp);
                        break;
                    }
                    case RequestPage:
                    {
                        Verb_Is_RequestPage(notification, sp, bconf);
                        break;
                    }

                    case Verb.Sales.RequestStay:
                    {
                        var pageRequest = notification.MessageAs<PageRequest>();
                        var stayRequest = pageRequest.Filter as StayRequest;

                        if (stayRequest == null)
                        {
                            throw new ArgumentNullException(nameof(pageRequest.Filter));
                        }

                        using var scope = sp.GetScope<SalesQueryService>(out var sales);

                        //TODO
                        var todocustomerId = 0;

                        //
                        //
                        var page = sales
                            .Find(stayRequest, todocustomerId)
                            .Page($"/booking", pageRequest.Page, pageRequest.PageSize);
                        //
                        //

                        Notify(new Notification(Omni, Verb.Sales.StayFound)
                        {
                            CorrelationGuid = notification.CorrelationGuid(),
                            Message = page
                        });

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