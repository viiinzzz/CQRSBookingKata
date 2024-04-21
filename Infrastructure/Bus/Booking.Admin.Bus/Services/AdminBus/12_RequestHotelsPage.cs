namespace BookingKata.Infrastructure.Bus.Admin;

public partial class AdminBus
{
    private void RequestHotelsPage(PageRequest request, out object? page)
    {
        using var scope = sp.GetScope<IAdminRepository>(out var adminRepository);
        using var scope2 = sp.GetScope<IGazetteerService>(out var geo);

        page = adminRepository
            .Hotels
            .Page(request.Path, request.Page, request.PageSize)
            .IncludeGeoIndex(bconf.PrecisionMaxKm, geo);
    }
}