namespace VinZ.GeoIndexing;

public interface IGazetteerService
{
    TReferer IncludeGeoIndex<TReferer>(TReferer referer, byte maxLevel)
        where TReferer : IHavePrimaryKeyAndPosition;

    IEnumerable<TReferer> IncludeGeoIndex<TReferer>(IEnumerable<TReferer> referers, double precisionMaxKm)
        where TReferer : IHavePrimaryKeyAndPosition;


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


    void AddReferer<TReferer>(TReferer referer, double? minKm, double? maxKm)
        where TReferer : IHavePrimaryKeyAndPosition;
    void RemoveReferer<TReferer>(TReferer referer)
        where TReferer : IHavePrimaryKeyAndPosition;
    void CopyToReferers<TReferer, TReferer2>(TReferer referer, IEnumerable<TReferer2> referers2)
        where TReferer : IHavePosition, IHavePrimaryKey
        where TReferer2 : IHavePrimaryKey; 
    void CopyToReferer<TReferer, TReferer2>(TReferer referer, TReferer2 referer2)
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
