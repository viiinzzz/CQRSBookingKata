namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<BookingCommandService>(out var booking);

            var originator = notification.Originator;
            var correlationGuid = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid;

            try
            {
                switch (notification.Verb)
                {
                    case Verb.OpenHotelSeasonRequest:
                    {
                        var request = notification.Json == default
                            ? throw new Exception("invalid request")
                            : JsonConvert.DeserializeObject<OpenHotelSeasonRequest>(notification.Json);

                        var openingDate = request.openingDate == default
                            ? default
                            : DateTime.ParseExact(request.openingDate, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                        var closingDate = request.closingDate == default
                            ? default
                            : DateTime.ParseExact(request.closingDate, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                        //
                        //
                        booking.OpenHotelSeason(
                            openingDate, 
                            closingDate,
                            request.exceptRoomNumbers,

                            request.hotelId,
                            notification.CorrelationId1,
                            notification.CorrelationId2
                        );
                        //
                        //

                        Notify(new NotifyMessage(Bus.Recipient.Any, Verb.HotelSeasonOpened)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = new { }
                        });
                    }
                        break;


                    case Verb.BookRequest:
                    {
                        var request = notification.Json == default
                            ? throw new Exception("invalid request")
                            : JsonConvert.DeserializeObject<BookRequest>(notification.Json);

                        //
                        //
                        var id = booking.Book(
                            request.arrivalTime,
                            request.departureTime,

                            request.stayPropositionId,
                            notification.CorrelationId1,
                            notification.CorrelationId2
                        );
                        //
                        //

                        Notify(new NotifyMessage(Bus.Recipient.Any, Verb.BookConfirmed)
                        {
                            CorrelationGuid = correlationGuid,
                            Message = new { id }
                        });
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(originator, Bus.Verb.RequestProcessingError)
                {
                    CorrelationGuid = correlationGuid,
                    Message = new
                    {
                        request = notification.Json,
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        };
    }
}