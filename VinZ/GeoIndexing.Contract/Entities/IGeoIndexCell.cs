namespace VinZ.GeoIndexing;

public interface IGeoIndexCell
{
    long S2CellIdSigned { get; }
    byte S2Level { get; }
}