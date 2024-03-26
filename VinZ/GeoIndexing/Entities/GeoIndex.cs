namespace VinZ.GeoIndexing;

public record GeoIndex(
    long S2CellIdSigned, 
    byte S2Level,
    
    long RefererId,
    int RefererTypeHash,
    long RefererHash,

    int GeoIndexId = default
) : IGeoIndex;