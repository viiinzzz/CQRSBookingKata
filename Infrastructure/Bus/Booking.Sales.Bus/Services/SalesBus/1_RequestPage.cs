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

public partial class SalesBus
{
    private void Verb_Is_RequestPage(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PageRequest>();

        object? page;

        using var scope3 = sp.GetScope<ISalesRepository>(out var salesRepository);
        using var scope4 = sp.GetScope<IGazetteerService>(out var geo);

        switch (request.Path)
        {
            case "/sales/customers":
            {
                page = salesRepository
                    .Customers
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            case "/admin/vacancies":
            {
                page = salesRepository
                    .Vacancies
                    .Page(request.Path, request.Page, request.PageSize)
                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                break;
            }

            case "/admin/bookings":
            {
                page = salesRepository
                    .Bookings
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            default:
            {
                throw new NotImplementedException($"page request for path not supported: {request.Path}");
            }
        }

        Notify(new ResponseNotification(notification, page));
    }
}