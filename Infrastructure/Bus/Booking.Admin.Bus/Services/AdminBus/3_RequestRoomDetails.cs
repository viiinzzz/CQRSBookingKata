namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestHotelRoomDetails(IClientNotification notification)
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

        Notify(new ResponseNotification(Omni, RespondHotelRoomDetails)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = roomDetails
        });
    }
    
    private void Verb_Is_RequestSingleRoomDetails(IClientNotification notification)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var request = notification.MessageAs<Id>();

        //
        //
        var roomDetails = adminQueryService
            .GetSingleRoomDetails(request.id);
        //
        //

        Notify(new ResponseNotification(Omni, RespondSingleRoomDetails)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = roomDetails
        });
    } 
    
    
    private void Verb_Is_RequestManyRoomDetails(IClientNotification notification)
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

        Notify(new ResponseNotification(Omni, RespondSingleRoomDetails)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = roomDetails
        });
    }
}