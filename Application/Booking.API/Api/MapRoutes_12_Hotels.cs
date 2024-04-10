using static BookingKata.Services.Verb.Admin;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private static void MapRoutes_12_Hotels(RouteGroupBuilder admin)
    {
        var hotels = admin.MapGroup("/hotels"
            ).WithOpenApi();

        hotels.MapListMq<Hotel>("/", "/admin/hotels",
            Recipient.Admin, RequestPage, responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapPostMq<NewHotel>("/",
            Recipient.Admin, RequestCreateHotel, responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapGetMq<Hotel>("/{id}",
            Recipient.Admin, RequestFetchHotel, responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapPatchMq<UpdateHotel>("/{id}",
            Recipient.Admin, RequestModifyHotel, responseTimeoutSeconds
            ).WithOpenApi();

        hotels.MapDisableMq<Hotel>("/{id}",
            Recipient.Admin, RequestDisableHotel, responseTimeoutSeconds
            ).WithOpenApi();
    }
}