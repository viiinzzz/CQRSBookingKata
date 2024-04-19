using static BookingKata.Services.Verb.Admin;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_12_Hotels(RouteGroupBuilder admin)
    {
        var hotels = admin.MapGroup("/hotels"
            ).WithOpenApi();

        hotels.MapListMq<Hotel>("/", "/admin/hotels", filter:null,
            Recipient.Admin, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapPostMq<NewHotel>("/",
            Recipient.Admin, RequestCreateHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapGetMq<Hotel>("/{id}",
            Recipient.Admin, RequestFetchHotel, originator,
            responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapPatchMq<UpdateHotel>("/{id}",
            Recipient.Admin, RequestModifyHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapDisableMq<Hotel>("/{id}",
            Recipient.Admin, RequestDisableHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi();
    }
}