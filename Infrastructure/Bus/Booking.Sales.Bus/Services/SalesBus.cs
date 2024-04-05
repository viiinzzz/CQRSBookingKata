using Business.Common;

namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<BookingCommandService>(out var booking);
            using var scope2 = sp.GetScope<KpiQueryService>(out var kpi);

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

                        Notify(new NotifyMessage(AnyRecipient, Verb.HotelSeasonOpened)
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

                        var debitCardSecrets = new DebitCardSecrets(request.debitCardOwner, request.debitCardExpire, request.debitCardCCV);
                        //
                        //
                        var id = booking.Book
                        (
                            request.lastName,
                            request.firstName,

                            request.debitCardNumber,
                            debitCardSecrets,

                            request.customerId,
                            request.stayPropositionId,

                            notification.CorrelationId1,
                            notification.CorrelationId2
                        );
                        //
                        //

                        Notify(new NotifyMessage(AnyRecipient, Verb.BookConfirmed)
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
                Notify(new NotifyMessage(originator, RequestProcessingError)
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