namespace VinZ.GeoIndexing;

public record GeoProxy
(
    string TypeFullName,
    long PrimaryKey,
    Position? Position
) 
    : IHavePrimaryKey, IHavePosition
{
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }
}