namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestReceipt(IClientNotification notification)
    {
        var request = notification.MessageAs<ReceiptRequest>();

        var bookingId = request.referenceId;

        using var scope = sp.GetScope<IMoneyRepository> (out var money);

        try
        {
            var receiptId = (
                from receipt in money.Receipts
                    .Include(r => r.Invoice)
                    .ThenInclude(invoice => invoice.Quotation)

                where receipt.Invoice.Quotation.ReferenceId == bookingId

                select receipt.ReceiptId
            ).FirstOrDefault();


            Notify(new Notification(Omni, ReceiptFound)
            {
                CorrelationGuid = notification.CorrelationGuid(),
                Message = new { id = receiptId }
            });
        }
        catch (Exception e)
        {
            Notify(new Notification(Omni, ReceiptNotFound)
            {
                CorrelationGuid = notification.CorrelationGuid(),
                Message = new { bookingId }
            });
        }
    }
}