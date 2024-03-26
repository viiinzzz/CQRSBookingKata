namespace VinZ.GeoIndexing;

public static partial class S2GeometryHelper
{
    public static S2CellId ToS2CellId(this long cellId)
    {
        var id = cellId.MapSignedToUnsigned<long, ulong>();

        return new S2CellId(id);
    }

    public static string ToS2Hex(this S2CellId cellId)
    {
        return $"s2:{cellId.Id:x16}";
    }
}
