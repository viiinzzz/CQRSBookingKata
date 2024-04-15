namespace VinZ.GeoIndexing;

public static class GeoProxyHelper
{
    public static GeoProxy GetGeoProxy<TReferer>(this TReferer referer) where TReferer : IHavePrimaryKeyAndPosition
    {
        return new GeoProxy(typeof(TReferer).FullName, referer.PrimaryKey, referer.Position);
    }
}