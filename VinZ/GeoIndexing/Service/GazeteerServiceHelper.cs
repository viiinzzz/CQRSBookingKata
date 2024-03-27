namespace VinZ.GeoIndexing;

public static class GazeteerServiceHelper
{
    public static IEnumerable<TReferer> IncludeGeoIndex<TReferer>(this IEnumerable<TReferer> referers, double precisionMaxKm, IGazetteerService geo)
        where TReferer : IHavePosition, IHavePrimaryKey
    {
        return geo.IncludeGeoIndex(referers);
    }

    public static IHaveCollection<TReferer> IncludeGeoIndex<TReferer>(this IHaveCollection<TReferer> haveReferers, double precisionMaxKm, IGazetteerService geo)
        where TReferer : IHavePosition, IHavePrimaryKey
    {
        haveReferers.Collection = geo.IncludeGeoIndex(haveReferers.Collection);

        return haveReferers;
    }

    public static (int RefereTypeHash, long RefererHash) GetRefererHashes<TReferer>(this TReferer? referer)
        where TReferer : IHavePrimaryKey
    {
        var refererType = typeof(TReferer).FullName
                          ?? throw new ArgumentException("type must have FullName", nameof(TReferer));

        var refererTypeHash = refererType.GetHashCode();

        long refererHash = default;

        if (referer != null)
        {
            //refererHash = (referer.PrimaryKey, refererTypeHash).GetHashCode();

            refererHash = HashHelper.GetHashCode64(
                BitConverter.GetBytes(referer.PrimaryKey),
                BitConverter.GetBytes(refererTypeHash));
        }

        return (refererTypeHash, refererHash);
    }


    public static GeoIndexCellDistance EarthArcDist(this IGeoIndexCell cell1, IGeoIndexCell cell2)
    {
        var id1 = cell1.S2CellIdSigned.ToS2CellId();
        var id2 = cell2.S2CellIdSigned.ToS2CellId();

        var levelMin = (byte)Math.Min(id1.Level, id2.Level);

        var worseTolerance = levelMin.EarthMaxKmForS2Level();
        var bestTolerance = levelMin.EarthMinKmForS2Level();

        var p1 = id1.ToPoint();
        var p2 = id2.ToPoint();

        var angle = new S1Angle(p1, p2);

        var km = angle.Radians * S2LatLng.EarthRadiusMeters / 1000;

        return new GeoIndexCellDistance(km, bestTolerance, worseTolerance);
    }
}