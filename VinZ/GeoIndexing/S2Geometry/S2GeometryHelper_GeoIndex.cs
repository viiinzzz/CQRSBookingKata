namespace VinZ.GeoIndexing;

public static partial class S2GeometryHelper
{
    public static IEnumerable<GeoIndex> GetGeoIndexes<TReferer>(TReferer referer, double? minKm, double? maxKm) 
        where TReferer : IHavePrimaryKeyAndPosition
    {
        var (refererTypeHash, refererHash) = referer.GetRefererHashes();

        var p = referer.Position;

        if (!p.HasValue)
        {
            return Enumerable.Empty<GeoIndex>();
        }

        var range = new RangeGeoIndex(p.Value, minKm, maxKm);

        return range
            .AsCellIds()
            .Select(c => new GeoIndex(c.S2CellIdSigned, c.S2Level, referer.PrimaryKey, refererTypeHash, refererHash));
    }
}
