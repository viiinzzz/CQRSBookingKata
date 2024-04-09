namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus(IScopeProvider sp, BookingConfiguration bconf) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Admin);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {
                    case Verb.Admin.RequestRoomDetails:
                        
                        Verb_Is_RequestRoomDetails(notification, sp);
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