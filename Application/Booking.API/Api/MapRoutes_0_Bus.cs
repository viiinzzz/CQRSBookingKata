namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_0_Bus(WebApplication app)
    {
        var busGroup = app.MapGroup("/bus"
        ).ExcludeFromDescription();

        busGroup.MapPost("/{busId}/notify",
            (ParsableHexInt busId, [FromBody]ClientNotification2 notification, [FromServices]IMessageBus bus) =>
            {
                return bus.Notify(notification, busId.Value);
            }
        ).ExcludeFromDescription();

        // busGroup.MapPost("/{busId}/subscribe",
        //     (ParsableHexInt busId, [FromBody]SubscriptionRequest sub, [FromServices]IMessageBus bus) =>
        //     {
        //         bus.Subscribe(sub, busId.Value);
        //     }
        // ).ExcludeFromDescription();
        //
        // busGroup.MapPost("/{busId}/unsubscribe",
        //     (ParsableHexInt busId, [FromBody] SubscriptionRequest sub, [FromServices]IMessageBus bus) =>
        //     {
        //         bus.Unsubscribe(sub, busId.Value);
        //     }
        // ).ExcludeFromDescription();


        busGroup.MapPost("/notify",
            ([FromBody] ClientNotification2 notification, [FromServices]IMessageBus bus) =>
            {
                return bus.Notify(notification, 0);
            }
        ).ExcludeFromDescription();

        busGroup.MapPost("/subscribe",
            ([FromBody]SubscriptionRequest sub, [FromServices]IMessageBus bus) =>
            {
                bus.Subscribe(sub, 0);
            }
        ).ExcludeFromDescription();

        busGroup.MapPost("/unsubscribe",
            ([FromBody] SubscriptionRequest sub, [FromServices]IMessageBus bus) =>
            {
                bus.Unsubscribe(sub, 0);
            }
        ).ExcludeFromDescription();
    }
}