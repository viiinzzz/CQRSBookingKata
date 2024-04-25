namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string DemoTag = "Demo";

    private static void MapRoutes_0_Demo(WebApplication app)
    {
        var demo = app.MapGroup("/demo");

        demo.MapGet("/forward",
            async (
                int days,
                ParsableNullableDouble speedFactor,
                [FromServices] DemoService demos
            ) =>
            {
                var forward = async () =>
                {
                    var newTime = await demos.Forward(days, speedFactor.Value, CancellationToken.None);

                    return Results.Redirect("/");
                };

                return await forward.WithStackTrace();
            }
        ).WithOpenApi().WithTags(new[] { DemoTag });
    }
}