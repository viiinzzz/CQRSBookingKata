namespace VinZ.GeoIndexing;

public interface IHavePosition
{
    Position? Position { get; }


    IList<IGeoIndexCell> Cells { get; set; }
    string geoIndex { get; set; }
}