namespace VinZ.GeoIndexing;

public record struct GeoIndexCell(long S2CellIdSigned, byte S2Level) : IGeoIndexCell;
