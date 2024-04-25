namespace BookingKata.Infrastructure.Bus.Sales;

public partial class SalesBus
{
    private void Verb_Is_RequestPage(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PageRequest>();

        object? page;

        using var scope3 = sp.GetScope<ISalesRepository>(out var salesRepository);
        using var scope4 = sp.GetScope<IGazetteerService>(out var geo);

        switch (request.Path)
        {
            case "/admin/vacancies":
            {
                page = salesRepository
                    .Vacancies
                    .Page(request.Path, request.Page, request.PageSize)
                    .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);

                break;
            }

            case "/admin/bookings":
            {
                page = salesRepository
                    .Bookings
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