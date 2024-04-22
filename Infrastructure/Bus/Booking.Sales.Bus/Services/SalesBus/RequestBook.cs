namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestBook(IClientNotification notification)
    {
        var request = notification.MessageAs<BookRequest>();

        var debitCardSecrets = new DebitCardSecrets(request.debitCardOwner, request.debitCardExpire, request.debitCardCCV);

        var vendor = new VendorIdentifiers(request.vendorId, request.terminalId);

        using var scope = sp.GetScope<BookingCommandService>(out var booking);

        //
        //
        var id = booking.Book
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

        Notify(new ResponseNotification(Omni, Verb.Sales.BookConfirmed)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = new
            {
                id
            }
        });
    }
}