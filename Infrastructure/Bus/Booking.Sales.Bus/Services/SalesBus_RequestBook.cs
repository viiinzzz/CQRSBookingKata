namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestBook(IClientNotification notification, IScopeProvider sp)
    {
        var request = notification.MessageAs<BookRequest>();

        var debitCardSecrets =
            new DebitCardSecrets(request.debitCardOwner, request.debitCardExpire, request.debitCardCCV);

        using var scope = sp.GetScope<BookingCommandService>(out var booking);

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

        Notify(new NotifyMessage(Omni, Verb.Sales.BookConfirmed)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = new
            {
                id
            }
        });
    }
}