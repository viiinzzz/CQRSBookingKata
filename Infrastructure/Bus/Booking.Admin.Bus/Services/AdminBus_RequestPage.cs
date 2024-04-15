namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void Verb_Is_RequestPage(IClientNotification notification, IScopeProvider sp, BookingConfiguration bconf)
    {
        using var scope = sp.GetScope<IAdminRepository>(out var adminRepository);
        using var scope2 = sp.GetScope<IGazetteerService>(out var geo);

        var request = notification.MessageAs<PageRequest>();

        object? page;

        switch (request.Path)
        {
            case "/admin/hotels":
            {
                page = adminRepository
                    .Hotels
                    .Page(request.Path, request.Page, request.PageSize)
                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                break;
            }

            case "/admin/employees":
            {
                page = adminRepository
                    .Employees
                    .Page(request.Path, request.Page, request.PageSize);

                break;
            }

            case "/admin/geo/indexes":
            {
                page = ((GazetteerService)geo)
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