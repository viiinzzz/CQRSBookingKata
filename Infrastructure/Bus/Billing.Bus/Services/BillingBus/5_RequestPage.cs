namespace Support.Infrastructure.Bus.Billing;

public partial class BillingBus
{
    private void Verb_Is_RequestPage(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PageRequest>();

        object? page;

        using var scope2 = sp.GetScope<IMoneyRepository>(out var moneyRepository);

        switch (request.Path)
        {
            case "/money/payrolls":
            {
                page = moneyRepository
                    .Payrolls
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            case "/money/invoices":
            {
                page = moneyRepository
                    .Invoices
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            default:
            {
                throw new NotImplementedException($"page request for path not supported: {request.Path}");
            }
        }

        Notify(new ResponseNotification(page)
        {
            Originator = notification.Originator,
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}