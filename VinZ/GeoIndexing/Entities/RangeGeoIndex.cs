namespace VinZ.GeoIndexing;

public class RangeGeoIndex
{
    private readonly long?[] _indexes = new long?[31];

    public RangeGeoIndex(Position position, double? minKm, double? maxKm)
    {
        var (minLevel, maxLevel) = S2GeometryHelper.S2MinMaxLevelForKm(minKm, maxKm);

        foreach (var cid in position.AllGeoIndexCell())
        {
            if (cid.S2Level >= minLevel && cid.S2Level <= maxLevel)
            {
                _indexes[cid.S2Level] = cid.S2CellIdSigned;
            }
        }
    }

    public IEnumerable<GeoIndexCell> AsCellIds()
        => _indexes
            .Where(cellId => cellId.HasValue)
            .Select((cellId, level) => new GeoIndexCell(cellId!.Value, (byte)level));
}
