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
    private const string DemoTag = "Demo";

    private static void MapRoutes_8_Demo(WebApplication app)
    {
        var demo = app.MapGroup("/demo");

        demo.MapGet("/forward", async
            (
                int days,
                ParsableNullableDouble speedFactor,
                long serverId,
                long sessionId,

                [FromServices] DemoBus demos,
                [FromServices] IServerContextService serverContext
            ) 
                =>
            {
                if (serverId != serverContext.Id)
                {
                    return Results.BadRequest($"Invalid serverId {serverId}");
                }

                if (sessionId != serverContext.SessionId)
                {
                    return Results.BadRequest($"Invalid sessionId {sessionId}");
                }

                var forward = async () =>
                {
                    var newTime = await demos.Forward(days, speedFactor.Value, CancellationToken.None);

                    return Results.Redirect("/");
                };

                return await forward.WithStackTrace();
            }
        ).WithOpenApi().WithTags([DemoTag]);


        app.MapGet("/context/demo", async
            (
                [FromServices] DemoContextService demoContext
            ) 
                =>
            {
                return Results.Json(demoContext);
            }
        ).WithOpenApi().WithTags([DemoTag]);

    }
}