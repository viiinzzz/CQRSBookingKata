namespace VinZ.GeoIndexing;

public record GeoProxy
(
    string TypeFullName,
    long PrimaryKey,
    Position? Position
) 
    : IHavePrimaryKeyAndPosition
{
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }
}