/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace BookingKata.Infrastructure.Network;

public class PlanningBus(IScopeProvider sp, BookingConfiguration bconf) : MessageBusClientBase
{
    public override async Task Configure()
    {
        // await
        Subscribe(Recipient.Planning);
        // await
        Subscribe(Recipient.Sales, Verb.Sales.BookConfirmed);
        // await
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

                        var bookingId = notification.MessageAs<Id<Booking>>();

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
                Notify(new NegativeResponseNotification(notification, ex));
            }
        };
    }

    private void Verb_Is_RequestPage(IClientNotificationSerialized notification)
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

        Notify(new ResponseNotification(notification.Originator, Respond, page)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}