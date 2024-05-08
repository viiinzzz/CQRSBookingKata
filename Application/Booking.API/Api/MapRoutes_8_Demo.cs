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

                [FromServices] DemoService demos,
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
    }
}