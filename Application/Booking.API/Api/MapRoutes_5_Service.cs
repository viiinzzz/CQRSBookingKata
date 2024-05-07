namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string ServiceTag = "Service";

    private static void MapRoutes_5_Service(WebApplication app)
    {
        var service = app.MapGroup("/service"
                ).WithOpenApi().WithTags([RestrictedTag, ServiceTag]);

        var room = service.MapGroup("/room"
                ).WithOpenApi().WithTags([RestrictedTag, ServiceTag]);


        room.MapListMq<RoomServiceDuty>("/hotels/{hotelId}", "/service/room/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, ServiceTag]);
    }
}