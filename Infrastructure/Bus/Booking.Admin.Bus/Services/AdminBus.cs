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
                    case RequestPage:

                        Verb_Is_RequestPage(notification, sp, bconf);
                        break;


                    case RequestCreateEmployee:
                    
                        Verb_Is_RequestCreateEmployee(notification, sp);
                        break;

                    case RequestFetchEmployee:
                    
                        Verb_Is_RequestFetchEmployee(notification, sp);
                        break;

                    case RequestModifyEmployee:
                    
                        Verb_Is_RequestModifyEmployee(notification, sp);
                        break;

                    case RequestDisableEmployee:
                    
                        Verb_Is_RequestDisableEmployee(notification, sp);
                        break;


                    case RequestCreateHotel:
                    
                        Verb_Is_RequestCreateHotel(notification, sp);
                        break;

                    case RequestFetchHotel:
                    
                        Verb_Is_RequestFetchHotel(notification, sp);
                        break;

                    case RequestModifyHotel:
                    
                        Verb_Is_RequestModifyHotel(notification, sp);
                        break;

                    case RequestDisableHotel:
                    
                        Verb_Is_RequestDisableHotel(notification, sp);
                        break;


                    case RequestRoomDetails:

                        Verb_Is_RequestRoomDetails(notification, sp);
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