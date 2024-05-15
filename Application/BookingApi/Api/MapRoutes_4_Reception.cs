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
    private const string ReceptionTag = "Reception";

    private static void MapRoutes_4_Reception(WebApplication app)
    {
        var reception = app.MapGroup("/reception"
                ).WithOpenApi().WithTags([RestrictedTag, ReceptionTag]);


        reception.MapListMq<ReceptionCheck>("/planning/full/hotels/{hotelId}", "/reception/planning/full/hotels/{hotelId}", filter: null,
            //TODO parameterized string interpolation
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, ReceptionTag]);

        reception.MapListMq<ReceptionCheck>("/planning/today/hotels/{hotelId}", "/reception/planning/today/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, ReceptionTag]);

        reception.MapListMq<ReceptionCheck>("/planning/week/hotels/{hotelId}", "/reception/planning/week/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, ReceptionTag]);

        reception.MapListMq<ReceptionCheck>("/planning/month/hotels/{hotelId}", "/reception/planning/month/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, ReceptionTag]);

    }
}