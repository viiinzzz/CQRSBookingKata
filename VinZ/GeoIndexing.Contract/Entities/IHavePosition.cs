namespace VinZ.GeoIndexing;

public interface IHavePosition
{
    Position? Position { get; }


    IList<IGeoIndexCell> Cells { get; set; }
    string geoIndex { get; set; }

    string GetGeoIndexString()
    {
        return string.Join(" ", Cells.Select(c =>
            $"{c.S2CellIdSigned:x16}".Substring(0, 8)));
    }
}
