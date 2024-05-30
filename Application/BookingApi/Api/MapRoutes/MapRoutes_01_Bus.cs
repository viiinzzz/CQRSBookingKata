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

using System;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string BusTag = "Bus";

    private static void MapRoutes_01_Bus(WebApplication app)
    {
        var busGroup = app.MapGroup("/bus"
        ).ExcludeFromDescription();



        app.MapGet("/debug/ping",
            (IMessageBus bus) =>
            {
                return Results.Accepted($"/bus/{bus.GetHashCode().xby4()}", "Pong");
            }
        ).WithOpenApi().WithTags([BusTag]);


        busGroup.MapPost("/subscribe",
            ([FromBody] SubscriptionRequest sub, [FromServices] IMessageBus bus) =>
            {
                bus.Subscribe(sub, 0);
            }
        ).WithOpenApi().WithTags([BusTag]);

        busGroup.MapPost("/unsubscribe",
            ([FromBody] SubscriptionRequest sub, [FromServices] IMessageBus bus) =>
            {
                bus.Unsubscribe(sub, 0);
            }
        ).WithOpenApi().WithTags([BusTag]);



        busGroup.MapPost("/{busId}/notify",
            (ParsableHexInt busId, [FromBody]RequestNotification notification, [FromServices]IMessageBus bus) =>
            {
                var ack = bus.Notify(notification, busId.Value);

                if (ack.CorrelationId == null)
                {
                    return Results.UnprocessableEntity(new
                    {
                        correlationId = "undefined",
                        status = ack.Status.ToString(),
                        error = ack.data
                    });
                }

                if (!ack.Valid)
                {
                    return Results.UnprocessableEntity(new
                    {
                        correlationId = ack.CorrelationId.ToString(),
                        status = ack.Status.ToString(),
                        error = ack.data
                    });
                }

                return Results.Accepted(ack.CorrelationId.ToString(), ack);
            }
        ).ExcludeFromDescription();
        
        // busGroup.MapPost("/notify",//debug purpose
        //     ([FromBody] dynamic notification, [FromServices]IMessageBus bus) =>
        //     {
        //         var notificationStr = (string)notification.ToString();
        //         ClientRequestNotification? notification_ = JsonConvert.DeserializeObject<ClientRequestNotification>(notificationStr);
        //         // var notificationDict =(IDictionary<string, object>)JsonConvert.DeserializeObject<ExpandoObject>(notification.ToString());
        //         return bus.Notify(notification, 0);
        //     }
        // ).ExcludeFromDescription();

        busGroup.MapPost("/notify",
            ([FromBody] RequestNotification notification, [FromServices]IMessageBus bus) =>
            {
                var ack = bus.Notify(notification, 0);

                if (ack.CorrelationId == null)
                {
                    return Results.UnprocessableEntity(new
                    {
                        correlationId = "undefined",
                        status = ack.Status.ToString(),
                        error = ack.data
                    });
                }

                if (!ack.Valid || ack.CorrelationId == null)
                {
                    return Results.UnprocessableEntity(new
                    {
                        correlationId = ack.CorrelationId.ToString(),
                        status = ack.Status.ToString(),
                        error = ack.data
                    });
                }

                return Results.Accepted(ack.CorrelationId.ToString(), ack);
            }
        ).WithOpenApi().WithTags([BusTag]);



        app.MapGet("/context/server", async
                (
                    [FromServices] IServerContextService serverContext
                )
                =>
            {
                return Results.Json(serverContext);
            }
        ).WithOpenApi().WithTags([BusTag]);
    }
}