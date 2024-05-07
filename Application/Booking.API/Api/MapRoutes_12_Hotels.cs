using static BookingKata.Services.Verb.Admin;

namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string HotelsTag = "Hotels";

    private static void MapRoutes_12_Hotels(RouteGroupBuilder admin)
    {
        var hotels = admin.MapGroup("/hotels"
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag]);

        hotels.MapListMq<Hotel>("/", "/admin/hotels", filter:null,
            Recipient.Admin, RequestPage, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapPostMq<NewHotel>("/",
            Recipient.Admin, RequestCreateHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapGetMq<Hotel>("/{id}",
            Recipient.Admin, RequestFetchHotel, originator,
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapPatchMq<UpdateHotel>("/{id}",
            Recipient.Admin, RequestModifyHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);

        hotels.MapDisableMq<Hotel>("/{id}",
            Recipient.Admin, RequestDisableHotel, originator, 
            responseTimeoutSeconds
            ).WithOpenApi().WithTags([RestrictedTag, AdminTag, HotelsTag]);
    }
}