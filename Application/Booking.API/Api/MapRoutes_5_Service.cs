namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_5_Service(WebApplication app)
    {
        var service = app.MapGroup("/service")
            .WithOpenApi();

        var room = service.MapGroup("/room")
            .WithOpenApi();


        room.MapListMq<RoomServiceDuty>("/hotels/{hotelId}", "/service/room/hotels/{hotelId}",
            Recipient.Planning, RequestPage, responseTimeoutSeconds
        ).WithOpenApi();
    }
}