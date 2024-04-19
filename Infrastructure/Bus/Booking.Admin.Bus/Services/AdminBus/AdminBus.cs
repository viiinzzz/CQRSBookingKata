﻿namespace BookingKata.Infrastructure.Bus.Admin;

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
                        {
                            Verb_Is_RequestPage(notification);
                            break;
                        }

                    case RequestCreateEmployee:
                        {
                            Verb_Is_RequestCreateEmployee(notification);
                            break;
                        }

                    case RequestFetchEmployee:
                        {
                            Verb_Is_RequestFetchEmployee(notification);
                            break;
                        }

                    case RequestModifyEmployee:
                        {
                            Verb_Is_RequestModifyEmployee(notification);
                            break;
                        }

                    case RequestDisableEmployee:
                        {
                            Verb_Is_RequestDisableEmployee(notification);
                            break;
                        }


                    case RequestCreateHotel:
                        {
                            Verb_Is_RequestCreateHotel(notification);
                            break;
                        }

                    case RequestFetchHotel:
                        {
                            Verb_Is_RequestFetchHotel(notification);
                            break;
                        }

                    case RequestFetchHotelGeoProxy:
                        {
                            Verb_Is_RequestFetchHotelGeoProxy(notification);
                            break;
                        }

                    case RequestModifyHotel:
                        {
                            Verb_Is_RequestModifyHotel(notification);
                            break;
                        }

                    case RequestDisableHotel:
                        {
                            Verb_Is_RequestDisableHotel(notification);
                            break;
                        }


                    case RequestRoomDetails:
                        {
                            Verb_Is_RequestHotelRoomDetails(notification);
                            break;
                        }

                    case RequestSingleRoomDetails:
                        {
                            Verb_Is_RequestSingleRoomDetails(notification);
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