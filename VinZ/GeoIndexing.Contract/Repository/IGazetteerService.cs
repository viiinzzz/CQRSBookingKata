using System.Collections.Generic;

namespace VinZ.GeoIndexing;

public interface IGazetteerService
{
    TReferer IncludeGeoIndex<TReferer>(TReferer referer)
        where TReferer : IHavePosition, IHavePrimaryKey;

    IEnumerable<TReferer> IncludeGeoIndex<TReferer>(IEnumerable<TReferer> referers)
        where TReferer : IHavePosition, IHavePrimaryKey;


    IQueryable<City> QueryCities(string cityName, bool? approximateNameMatch, string? countryCode);
    (City?, double) NearestCity(IGeoIndexCell searchCell);

    IGeoIndexCell[] NewGeoIndex<TSearch>(TSearch search, double maxPrecisionKm, double? maxDistanceKm)
        where TSearch : IHavePosition;
    GeoIndexCellDistance EarthArcDist(IGeoIndexCell cell1, IGeoIndexCell cell2);


    IGeoIndexCell[] CacheGeoIndex<TEntity>(TEntity entity, double precisionMaxKm)
        where TEntity : IHavePosition;
    IGeoIndexCell CacheGeoIndex<TEntity>(TEntity entity)
        where TEntity : IHavePosition;
    void ClearCacheGeoIndex();


    void AddReferer<TReferer>(TReferer referer, double? minKm, double? maxKm, bool scoped)
        where TReferer : IHavePosition, IHavePrimaryKey;
    void RemoveReferer<TReferer>(TReferer referer, bool scoped)
        where TReferer : IHavePosition, IHavePrimaryKey;
    void CopyToReferers<TReferer, TReferer2>(TReferer referer, IEnumerable<TReferer2> referers2, bool scoped)
        where TReferer : IHavePosition, IHavePrimaryKey
        where TReferer2 : IHavePrimaryKey;
    IGeoIndexCell? RefererGeoIndex<TReferer>(TReferer referer)
        where TReferer : IHavePrimaryKey;
    IList<IGeoIndexCell> RefererAllGeoIndex<TReferer>(TReferer referer) 
        where TReferer : IHavePrimaryKey;


    IQueryable<int> GetMatchingRefererIntIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey;
    IQueryable<long> GetMatchingRefererLongIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey;
}
