namespace BookingKata.Infrastructure.Bus.Admin;

public class AdminBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Admin);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);
            using var scope2 = sp.GetScope<IAdminRepository>(out var adminRepository);
            using var scope3 = sp.GetScope<IGazetteerService>(out var geo);

            var originator = notification.Originator;
            var correlationGuid = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid;

            try
            {
                switch (notification.Verb)
                {
                    case Verb.Admin.RequestRoomDetails:
                    {
                        var request = notification.MessageAs<RoomDetailsRequest>();
                       
                        //
                        //
                        var roomDetails = adminQueryService
                            .GetRoomDetails(request.hotelId, request.exceptRoomNumbers)
                            .ToArray();
                        //
                        //
                        
                        Notify(new NotifyMessage(Omni, Verb.Admin.RespondRoomDetails)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = roomDetails
                        });

                        break;
                    }


                    case RequestPage:
                    {
                        var request = notification.MessageAs<PageRequest>();

                        object? page;

                        switch (request.Path)
                        {
                            case "/admin/hotels":
                            {
                                page = adminRepository
                                    .Hotels
                                    .Page(request.Path, request.Page, request.PageSize)
                                    .IncludeGeoIndex(PrecisionMaxKm, geo);

                                break;
                            }

                            case "/admin/employees":
                            {
                                page = adminRepository
                                    .Employees
                                    .Page(request.Path, request.Page, request.PageSize);

                                break;
                            }

                            case "/admin/geo/indexes":
                            {
                                page = ((GazetteerService)geo)
                                    .Indexes
                                    .Page(request.Path, request.Page, request.PageSize);

                                break;
                            }

                            default:
                            {
                                throw new NotImplementedException($"page request for path not supported: {request.Path}");
                            }
                        }

                        Notify(new NotifyMessage(originator, RespondPage)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = page
                        });

                        break;
                    }


                   


                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(originator, ErrorProcessingRequest)
                {
                    CorrelationGuid = correlationGuid,
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