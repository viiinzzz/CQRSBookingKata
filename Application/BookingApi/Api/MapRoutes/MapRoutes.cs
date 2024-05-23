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
    const int responseTimeoutSeconds = 120;

    public static void MapRoutes(WebApplication app, bool isMainContainer)
    {

        // if (app.Environment.IsDevelopment())
        // {
        app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
        {
            return string.Join("\n", endpointSources

                .SelectMany(source => source.Endpoints)

                .Select(endpoint => new {
                    Pattern = (endpoint as RouteEndpoint)?.RoutePattern?.RawText,
                    Method = string.Join(',', endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods ?? [])
                })
                .SelectMany(r => r.Method.Split(',').Select(m => r with { Method = m }))
                
                .OrderBy(r => ((r.Pattern ?? string.Empty).StartsWith('/') ? r.Pattern![1..] : r.Pattern)?.ToLower())
                .ThenBy(r => r.Method?.ToLower())
                
                .Select(r => $"{(r.Pattern.StartsWith('/') ? "" : '/')}{r.Pattern?.ToLower()} {r.Method}")
            
            );
        });

        app.MapGet("/debug/routes/details", (IEnumerable<EndpointDataSource> endpointSources) =>
        {
            return endpointSources

                .SelectMany(es => es.Endpoints).Select(endpoint =>
                {
                    var routeName = endpoint.Metadata.OfType<RouteNameMetadata>()
                        .FirstOrDefault();

                    var httpMethods = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();

                    var routeEndpoint = endpoint as RouteEndpoint;

                    return new
                    {
                        Name = routeName?.RouteName,

                        Method = httpMethods == null ? string.Empty : string.Join(",", httpMethods.HttpMethods), // [GET, POST, ...]

                        Pattern = routeEndpoint?.RoutePattern?.RawText,

                        Segments = routeEndpoint?.RoutePattern?.PathSegments?.ToArray(),

                        Parameters = routeEndpoint?.RoutePattern?.Parameters?.ToArray(),

                        Precedence = new
                        {
                            Inbound = routeEndpoint?.RoutePattern?.InboundPrecedence,
                            Outbound = routeEndpoint?.RoutePattern?.OutboundPrecedence
                        }
                    };
                })
                .SelectMany(r => r.Method.Split(',').Select(m => r with { Method = m }))

                .OrderBy(r => r.Pattern?.ToLower()).ThenBy(r => r.Method?.ToLower());
        });

        MapRoutes_0_Bus(app);

        if (!isMainContainer)
        {
            return;
        }


        app.MapGet("/debug/subscribe",
            ([FromServices] IMessageBus bus) =>
            {
                var urls = (bus as MqServer)?.GetSubscribeUrls();

                if (urls == null)
                {
                    return "";
                }

                return string.Join(Environment.NewLine, urls);
            }
        ).WithOpenApi().WithTags([BusTag]);


        MapRoutes_1_Admin(app, out var admin);
        {
            MapRoutes_11_Employees(admin);
            MapRoutes_12_Hotels(admin);
        }

        MapRoutes_2_Money(app);

        MapRoutes_3_Sales(app);

        MapRoutes_4_Reception(app);

        MapRoutes_5_Service(app);

        MapRoutes_6_Booking(app);

        MapRoutes_8_Demo(app);

        MapRoutes_9_Swagger(app);


    }
}