namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_5_Service(WebApplication app)
    {
        var service = app.MapGroup("/service")
            .WithOpenApi();

        var room = service.MapGroup("/room")
            .WithOpenApi();


        room.MapListMq<RoomServiceDuty>("/hotels/{hotelId}", "/service/room/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
        ).WithOpenApi();
    }
}