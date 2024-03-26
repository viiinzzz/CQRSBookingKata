namespace VinZ.GeoIndexing;

public static partial class S2GeometryHelper
{
    public static GeoIndexCell GeoIndexCell(this S2CellId cid) => new GeoIndexCell
    (
        S2CellIdSigned: cid.Id.MapUnsignedToSigned<ulong, long>(),
        S2Level: (byte)cid.Level
    );


    public static GeoIndexCell GeoIndexCell(this Position p) => GeoIndexCell
    (
        cid: new S2Cell(
            S2LatLng.FromDegrees(p.Latitude, p.Longitude)
            ).Id
    );


    public static S2CellId S2CellId(this GeoIndexCell cids) => cids.S2CellIdSigned.ToS2CellId();


    public static IEnumerable<GeoIndexCell> AllGeoIndexCell(this Position p)
    {
        var l = S2LatLng.FromDegrees(p.Latitude, p.Longitude);
        var c = new S2Cell(l);
        S2CellId? cid = default;

        while (true)
        {
            cid = cid?.Parent ?? c.Id;

            var id = GeoIndexCell(cid.Value);

            yield return id;

            if (id.S2Level <= 0) yield break;
        }
    }
}
