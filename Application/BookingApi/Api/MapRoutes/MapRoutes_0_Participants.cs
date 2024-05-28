namespace BookingKata.API;

public static partial class ApiMethods
{
    public static void MapParticipantRoutes(WebApplication app)
    {
        MapRoutes_01_Bus(app);


        app.MapGet("/debug/subscribe",
            ([FromServices] IMessageBus bus) =>
            {
                var urls = (bus as MqServer)?.GetSubscribeUrls();

                if (urls == null)
                {
                    return string.Empty;
                }

                var subscribe = string.Join(Environment.NewLine, urls);

                return subscribe;
            }
        ).WithOpenApi().WithTags([BusTag]);



        app.MapDebugRoutes();
    }
}