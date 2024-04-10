namespace BookingKata.Infrastructure.Network;

public class PlanningBus(IScopeProvider sp, BookingConfiguration bconf) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Planning);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {
                    case RequestPage:
                    {
                        using var scope = sp.GetScope<PlanningQueryService>(out var planning);
                        using var scope2 = sp.GetScope<IGazetteerService>(out var geo);

                        var request = notification.MessageAs<PageRequest>();

                        object? page;

                        switch (request.Path)
                        {
                            case var path when Regex.IsMatch(path, @"^/planning/full/hotels/(\d+)$"):
                            {
                                var hotelId = int.Parse(Regex.Replace(path, @".*/(\d+)$", "$1"));

                                page = planning
                                    .GetReceptionFullPlanning(hotelId)
                                    .Page(path, request.Page, request.PageSize)
                                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                                break;
                            }

                            case var path when Regex.IsMatch(path, @"^/planning/today/hotels/(\d+)$"):
                            {
                                var hotelId = int.Parse(Regex.Replace(path, @".*/(\d+)$", "$1"));

                                page = planning
                                    .GetReceptionTodayPlanning(hotelId)
                                    .Page(path, request.Page, request.PageSize)
                                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                                break;
                            }

                            case var path when Regex.IsMatch(path, @"^/planning/week/hotels/(\d+)$"):
                            {
                                var hotelId = int.Parse(Regex.Replace(path, @".*/(\d+)$", "$1"));

                                page = planning
                                    .GetReceptionWeekPlanning(hotelId)
                                    .Page(path, request.Page, request.PageSize)
                                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                                break;
                            }

                            case var path when Regex.IsMatch(path, @"^/planning/month/hotels/(\d+)$"):
                            {
                                var hotelId = int.Parse(Regex.Replace(path, @".*/(\d+)$", "$1"));

                                page = planning
                                    .GetReceptionMonthPlanning(hotelId)
                                    .Page(path, request.Page, request.PageSize);

                                break;
                            }


                            case var path when Regex.IsMatch(path, @"^/service/room/hotels/(\d+)$"):
                            {
                                var hotelId = int.Parse(Regex.Replace(path, @".*/(\d+)$", "$1"));

                                page = planning
                                    .GetServiceRoomPlanning(hotelId)
                                    .Page(path, request.Page, request.PageSize);

                                break;
                            }

                            default:
                            {
                                throw new NotImplementedException($"page request for path not supported: {request.Path}");
                            }
                        }

                        Notify(new NotifyMessage(notification.Originator, Respond)
                        {
                            CorrelationGuid = notification.CorrelationGuid(),
                            Message = page
                        });
                    }


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