namespace Support.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPage(IClientNotification notification)
    {
        using var scope2 = sp.GetScope<IGazetteerService>(out var geo);

        var request = notification.MessageAs<PageRequest>();

        object? page;

        switch (request.Path)
        {
            case "/admin/geo/indexes":
            {
                page = ((GazetteerServiceBase)geo)
                    .Indexes
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            default:
            {
                throw new NotImplementedException($"page request for path not supported: {request.Path}");
            }
        }

        Notify(new Notification(notification.Originator, Respond)
        {
            CorrelationGuid = notification.CorrelationGuid(),
            Message = page
        });
    }
}