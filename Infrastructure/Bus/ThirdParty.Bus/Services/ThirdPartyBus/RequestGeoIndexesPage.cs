namespace Support.Infrastructure.Bus.ThirdParty;

public partial class ThirdPartyBus
{
    private void RequestGeoIndexesPage(PageRequest request, out object? page)
    {
        using var scope = sp.GetScope<IGazetteerService>(out var geo);

        page = ((GazetteerServiceBase)geo)
            .Indexes
            .Select(index => (GeoIndex)index)
            .Page(request.Path, request.Page, request.PageSize);
    }
}