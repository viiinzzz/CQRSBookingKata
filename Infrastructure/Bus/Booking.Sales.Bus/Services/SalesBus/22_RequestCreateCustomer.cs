namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestCreateCustomer(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<CreateCustomerRequest>();

        using var scope = sp.GetScope<ISalesRepository>(out var repo);

        var customerId = repo.CreateCustomer(request.EmailAddress);

        var id = new Id(customerId);

        Notify(new ResponseNotification(notification.Originator, Verb.Sales.CustomerCreated, id)
        {
            CorrelationId1 = notification.CorrelationId1,
            CorrelationId2 = notification.CorrelationId2
        });
    }
}