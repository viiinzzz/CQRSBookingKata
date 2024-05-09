namespace Support.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPage(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PageRequest>();

        object? page;

        switch (request.Path)
        {

            case "/admin/geo/indexes":
            {
                RequestGeoIndexesPage(request, out page);
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