namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestOpenHotelSeason(IClientNotification notification)
    {
        var request = notification.MessageAs<OpenHotelSeasonRequest>();

        var openingDate = DateTime.ParseExact(request.openingDate,
            "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

        var closingDate = DateTime.ParseExact(request.closingDate,
            "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

        using var scope = sp.GetScope<BookingCommandService>(out var booking);

        //
        //
        booking.OpenHotelSeason(
            request.hotelId,
            request.exceptRoomNumbers,
            openingDate,
            closingDate
            //
            // notification.CorrelationId1,
            // notification.CorrelationId2
        );
        //
        //

        var opening = new
        {
            request.hotelId,
            openingDate,
            closingDate
        };

        Notify(new Notification(Omni, Verb.Sales.HotelSeasonOpening)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = opening
        });
    }
}