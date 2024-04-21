using Business.Common;

namespace BookingKata.Infrastructure.Network;

public class PlanningBus(IScopeProvider sp, BookingConfiguration bconf) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Planning);
        Subscribe(Recipient.Sales, Verb.Sales.BookConfirmed);
        Subscribe(Recipient.Sales, Verb.Sales.BookCancelled);

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

                    case Verb.Sales.BookConfirmed:
                    {
                        using var scope = sp.GetScope<PlanningCommandService>(out var planning);

                        var booking = notification.MessageAs<Booking>();

                        planning.PlanForBooking(booking);

                        break;
                    }

                    case Verb.Sales.BookCancelled:
                    {
                        using var scope = sp.GetScope<PlanningCommandService>(out var planning);

                        var bookingId = notification.MessageAs<Id>();

                        planning.CancelPlanForBooking(bookingId.id);

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
                Notify(new ResponseNotification(notification.Originator, ErrorProcessingRequest)
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

    private void Verb_Is_RequestPage(IClientNotification notification)
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
                    .Page(path, request.Page, request.PageSize)
                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                break;
            }


            case var path when Regex.IsMatch(path, @"^/service/room/hotels/(\d+)$"):
            {
                var hotelId = int.Parse(Regex.Replace(path, @".*/(\d+)$", "$1"));

                page = planning
                    .GetServiceRoomPlanning(hotelId)
                    .Page(path, request.Page, request.PageSize)
                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                break;
            }

            default:
            {
                throw new NotImplementedException($"page request for path not supported: {request.Path}");
            }
        }

        Notify(new ResponseNotification(notification.Originator, Respond)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = page
        });
    }
}