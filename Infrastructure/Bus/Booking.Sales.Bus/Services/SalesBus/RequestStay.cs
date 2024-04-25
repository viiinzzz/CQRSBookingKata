namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestStay(IClientNotificationSerialized notification)
    {
        var pageRequest = notification.MessageAs<PageRequest>();
        var stayRequest = pageRequest.Filter as StayRequest;

        if (stayRequest == null)
        {
            throw new ArgumentNullException(nameof(pageRequest.Filter));
        }

        using var scope = sp.GetScope<SalesQueryService>(out var sales);

        //TODO!!!!
        var currentCustomerId = 9999_9999;

        //
        //
        var page = sales
            .FindStay(stayRequest, currentCustomerId)
            .Page($"/booking", pageRequest.Page, pageRequest.PageSize);
        //
        //

        Notify(new ResponseNotification(Omni, Verb.Sales.StayFound, page)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}