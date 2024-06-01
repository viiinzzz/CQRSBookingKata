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
    private void Verb_Is_RequestHotelKpi(IClientNotificationSerialized notification)
    {
        var id = notification.MessageAs<int>();

        using var scope = sp.GetScope<KpiQueryService>(out var kpi);

        //
        //
        var indicators = new KeyPerformanceIndicators
        {
            hotelId = id,
            totalBookingCount = kpi.TotalBookingCount(id),
            occupancyRate = kpi.GetOccupancyRate(id),
        };
        //
        //

        Notify(notification.Response(new ResponseOptions { 
            Recipient = notification.Originator,
            Verb = Verb.Sales.RespondHotelKpi, 
            MessageObj = indicators
        }));
    }

    private void Verb_Is_RequestHotelChainKpi(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<KpiQueryService>(out var kpi);

        //
        //
        var indicators = new KeyPerformanceIndicators
        {
            totalBookingCount = kpi.TotalBookingCount(null),
            occupancyRate = kpi.GetOccupancyRate(null),
        };
        //
        //

        Notify(notification.Response(new ResponseOptions
        {
            Recipient = notification.Originator,
            Verb = Verb.Sales.RespondHotelChainKpi,
            MessageObj = indicators
        }));
    }
}