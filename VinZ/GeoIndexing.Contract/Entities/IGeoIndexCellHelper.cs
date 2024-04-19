namespace VinZ.GeoIndexing;

public static class IGeoIndexCellHelper
{
    public static IGeoIndexCell TopCell(this IEnumerable<IGeoIndexCell> cells)
    {
        return cells.OrderByDescending(cell => cell.S2Level).Single();
    }
}