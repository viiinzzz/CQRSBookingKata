namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestBook(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<BookRequest>();

        var debitCardSecrets = new DebitCardSecrets(request.debitCardOwner, request.debitCardExpire, request.debitCardCCV);

        var vendor = new VendorIdentifiers(request.vendorId, request.terminalId);

        using var scope = sp.GetScope<BookingCommandService>(out var booking);

        //
        //
        var bookingId = booking.Book
        (
            request.lastName,
            request.firstName,

            request.debitCardNumber,
            debitCardSecrets,
            vendor,

            request.customerId,
            request.stayPropositionId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        var id = new Id(bookingId);

        Notify(new ResponseNotification(Omni, Verb.Sales.BookConfirmed, id)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}