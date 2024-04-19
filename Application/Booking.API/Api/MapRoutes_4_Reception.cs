using BookingKata.Admin;
using BookingKata.Sales;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_4_Reception(WebApplication app)
    {
        var reception = app.MapGroup("/reception")
        .WithOpenApi();




        reception.MapListMq<ReceptionCheck>("/planning/full/hotels/{hotelId}", "/reception/planning/full/hotels/{hotelId}", filter: null,
            //TODO parameterized string interpolation
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
        ).WithOpenApi();

        reception.MapListMq<ReceptionCheck>("/planning/today/hotels/{hotelId}", "/reception/planning/today/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
        ).WithOpenApi();

        reception.MapListMq<ReceptionCheck>("/planning/week/hotels/{hotelId}", "/reception/planning/week/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
        ).WithOpenApi();

        reception.MapListMq<ReceptionCheck>("/planning/month/hotels/{hotelId}", "/reception/planning/month/hotels/{hotelId}", filter: null,
            Recipient.Planning, RequestPage, originator,
            responseTimeoutSeconds
        ).WithOpenApi();

    }
}