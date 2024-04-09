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
                    
                        Verb_Is_RequestOpenHotelSeason(notification, sp);
                        break;

                    case Verb.Sales.RequestBook:
                    
                        Verb_Is_RequestBook(notification, sp);
                        break;
                    
                    case Verb.Sales.RequestKpi:
                    
                        Verb_Is_RequestKpi(notification, sp);
                        break;

                    case RequestPage:
                    
                        Verb_Is_RequestPage(notification, sp, bconf);
                        break;
                    
                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(notification.Originator, ErrorProcessingRequest)
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