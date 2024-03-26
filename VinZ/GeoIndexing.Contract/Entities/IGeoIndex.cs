namespace VinZ.GeoIndexing;

public interface IGeoIndex : IGeoIndexCell
{
    long RefererId { get; }
    int RefererTypeHash { get; }
    long RefererHash { get; }
    
    int GeoIndexId { get; }
}
