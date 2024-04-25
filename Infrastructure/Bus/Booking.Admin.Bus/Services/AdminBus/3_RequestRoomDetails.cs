namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestHotelRoomDetails(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var request = notification.MessageAs<RoomDetailsRequest>();

        if (request.hotelId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(request.hotelId));
        }

        //
        //
        var roomDetails = adminQueryService
            .GetHotelRoomDetails(request.hotelId.Value, request.exceptRoomNumbers, request.onlyRoomNumbers)
            .ToArray();
        //
        //

        Notify(new ResponseNotification(Omni, RespondHotelRoomDetails, roomDetails)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
    
    private void Verb_Is_RequestSingleRoomDetails(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var request = notification.MessageAs<Id>();

        //
        //
        var roomDetails = adminQueryService
            .GetSingleRoomDetails(request.id);
        //
        //

        Notify(new ResponseNotification(Omni, RespondSingleRoomDetails, roomDetails)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    } 
    
    
    private void Verb_Is_RequestManyRoomDetails(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var request = notification.MessageAs<RoomDetailsRequest>();

        if (request.hotelId != null)
        {
            throw new ArgumentException(ReferenceUnexpected, nameof(request.hotelId));
        }

        if (request.exceptRoomNumbers != null)
        {
            throw new ArgumentException(ReferenceUnexpected, nameof(request.exceptRoomNumbers));
        }

        if (request.onlyRoomNumbers == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(request.onlyRoomNumbers));
        }

        //
        //
        var roomDetails = adminQueryService
            .GetManyRoomDetails(request.onlyRoomNumbers);
        //
        //

        Notify(new ResponseNotification(Omni, RespondSingleRoomDetails, roomDetails)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}