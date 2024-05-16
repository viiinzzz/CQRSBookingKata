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
    private const string originator = "Api";


    private const string RestrictedTag = "Restricted";
    private const string AdminTag = "Admin";

    private static void MapRoutes_1_Admin(WebApplication app, out RouteGroupBuilder admin)
    {
        admin = app.MapGroup("/admin"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);


        admin.MapListMq<Vacancy>("/vacancies", "/admin/vacancies", filter: null, 
            Recipient.Sales, RequestPage, originator,
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        admin.MapListMq<Shared.Booking>("/bookings", "/admin/bookings", filter: null,
            Recipient.Sales, RequestPage, originator, 
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        admin.MapListMq<GeoIndex>("/geo/indexes", "/admin/geo/indexes", filter: null,
            Support.Services.ThirdParty.Recipient, RequestPage, originator,
            responseTimeoutSeconds: responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);


        admin.MapGet("/hotels/{id}/kpi", async
            (
                int id,
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var kpi = await mq.Ask<KeyPerformanceIndicators>(
                originator, Recipient.Sales, Verb.Sales.RequestHotelKpi, id,
                requestCancel, responseTimeoutSeconds);


            var html = @$"
<h1>B O O K I N G  API</h1>
<h2>Key Performance Indicators</h2>
<ul>
<li>Occupancy Rate: {kpi.occupancyRate:P}
</ul>
";

            return Results.Content(html, "text/html");
        }).WithOpenApi().WithTags([RestrictedTag, AdminTag]);


        admin.MapGet("/kpi", async
            (
                [FromServices] IMessageBus mq,
                CancellationToken requestCancel
            )
            =>
        {
            var kpi = await mq.Ask<KeyPerformanceIndicators>(
                originator, Recipient.Sales, Verb.Sales.RequestHotelChainKpi, null,
                requestCancel, responseTimeoutSeconds);


            var html = @$"
<h1>B O O K I N G  API</h1>
<h2>Key Performance Indicators</h2>
<ul>
<li>Occupancy Rate: {kpi.occupancyRate:P}
</ul>
";

            return Results.Content(html, "text/html");
        }).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

    }
}