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

using static BookingKata.Services.Verb.Admin;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string HotelsTag = "Hotels";

    private static void MapRoutes_12_Hotels(RouteGroupBuilder admin)
    {
        var hotels = admin.MapGroup("/hotels"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        hotels.MapListMq<Hotel>("/", "/admin/hotels", filter:null,
            Recipient.Admin, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapPostMq<NewHotel>("/",
            Recipient.Admin, RequestCreateHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapGetMq<Hotel>("/{id}",
            Recipient.Admin, RequestFetchHotel, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapPatchMq<ModifyHotel>("/{id}",
            Recipient.Admin, RequestModifyHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapDisableMq<Hotel>("/{id}",
            Recipient.Admin, RequestDisableHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);
    }
}