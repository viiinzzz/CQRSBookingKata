using Newtonsoft.Json;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_0_Bus(WebApplication app)
    {
        var busGroup = app.MapGroup("/bus"
        ).ExcludeFromDescription();



        busGroup.MapPost("/subscribe",
            ([FromBody] SubscriptionRequest sub, [FromServices] IMessageBus bus) =>
            {
                bus.Subscribe(sub, 0);
            }
        ).ExcludeFromDescription();

        busGroup.MapPost("/unsubscribe",
            ([FromBody] SubscriptionRequest sub, [FromServices] IMessageBus bus) =>
            {
                bus.Unsubscribe(sub, 0);
            }
        ).ExcludeFromDescription();



        busGroup.MapPost("/{busId}/notify",
            (ParsableHexInt busId, [FromBody]ClientRequestNotification notification, [FromServices]IMessageBus bus) =>
            {
                return bus.Notify(notification, busId.Value);
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
            ([FromBody] ClientRequestNotification notification, [FromServices]IMessageBus bus) =>
            {
                return bus.Notify(notification, 0);
            }
        ).ExcludeFromDescription();

    }
}