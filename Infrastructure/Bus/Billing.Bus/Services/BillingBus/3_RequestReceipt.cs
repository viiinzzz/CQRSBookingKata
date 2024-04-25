namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestReceipt(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<ReceiptRequest>();

        var bookingId = request.referenceId;

        using var scope = sp.GetScope<IMoneyRepository> (out var money);

        var referenceId = new { bookingId };

        try
        {
            var receiptId = (
                from receipt in money.Receipts
                    .Include(r => r.Invoice)
                    .ThenInclude(invoice => invoice.Quotation)

                where receipt.Invoice.Quotation.ReferenceId == bookingId

                select receipt.ReceiptId
            ).FirstOrDefault();

            var id = new Id(receiptId);

            Notify(new ResponseNotification(Omni, ReceiptFound, id)
            {
                CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
            });
        }
        catch (Exception e)
        {
            Notify(new ResponseNotification(Omni, ReceiptNotFound, referenceId)
            {
                CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
            });
        }
    }
}