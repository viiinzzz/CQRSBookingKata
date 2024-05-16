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

namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string ServiceTag = "Service";

    private static void MapRoutes_5_Service(WebApplication app)
    {
        var service = app.MapGroup("/service"
                ).WithOpenApi().WithTags([RestrictedTag, ServiceTag]);

        var room = service.MapGroup("/room"
                ).WithOpenApi().WithTags([RestrictedTag, ServiceTag]);


        room.MapListMq<RoomServiceDuty>("/hotels/{hotelId}", "/service/room/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, ServiceTag]);
    }
}