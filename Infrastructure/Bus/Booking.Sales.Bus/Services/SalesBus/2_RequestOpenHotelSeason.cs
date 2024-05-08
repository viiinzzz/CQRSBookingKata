namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestOpenHotelSeason(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<OpenHotelSeasonRequest>();

        var openingDateUtc = request.openingDateUtc.DeserializeUniversal_ThrowIfNull(nameof(request.openingDateUtc));
        var closingDateUtc = request.closingDateUtc.DeserializeUniversal_ThrowIfNull(nameof(request.closingDateUtc));

        using var scope = sp.GetScope<BookingCommandService>(out var booking);

        //
        //
        booking.OpenHotelSeason
        (
            request.hotelId,
            request.exceptRoomNumbers,
            openingDateUtc,
            closingDateUtc
            //
            // notification.CorrelationId1,
            // notification.CorrelationId2
        );
        //
        //

        var opening = new HotelOpening
        {
            hotelId = request.hotelId,
            openingDate = openingDateUtc.SerializeUniversal(),
            closingDate = closingDateUtc.SerializeUniversal()
        };

        Notify(new ResponseNotification(Omni, Verb.Sales.HotelSeasonOpening, opening)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}