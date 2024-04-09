namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestRoomDetails(IClientNotification notification,IScopeProvider sp)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var request = notification.MessageAs<RoomDetailsRequest>();

        //
        //
        var roomDetails = adminQueryService
            .GetRoomDetails(request.hotelId, request.exceptRoomNumbers)
            .ToArray();
        //
        //

        Notify(new NotifyMessage(Omni, Verb.Admin.RespondRoomDetails)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = roomDetails
        });
    }
}