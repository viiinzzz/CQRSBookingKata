namespace VinZ.GeoIndexing;

public interface IGazeteerService
{
    IQueryable<City> QueryCities(string cityName, bool? approximateNameMatch, string? countryCode);
    (City?, double) NearestCity(IGeoIndexCell searchCell);

    void ClearCacheGeoIndex();
    IGeoIndexCell[] SearchGeoIndex<TSearch>(TSearch search, double maxPrecisionKm, double? maxDistanceKm)
        where TSearch : IHavePosition;
    IGeoIndexCell[] GeoIndex<TEntity>(TEntity entity, double maxPrecisionKm)
        where TEntity : IHavePosition;
    IGeoIndexCell GeoIndex<TEntity>(TEntity entity)
        where TEntity : IHavePosition;
    IGeoIndexCell? RefererGeoIndex<TReferer>(TReferer referer)
        where TReferer : IHavePrimaryKey;

    GeoIndexCellDistance EarthArcDist(IGeoIndexCell cell1, IGeoIndexCell cell2);

    void AddReferer<TReferer>(TReferer referer, double? minKm, double? maxKm, bool scoped) 
        where TReferer: IHavePosition, IHavePrimaryKey;
    void RemoveReferer<TReferer>(TReferer referer, bool scoped) 
        where TReferer: IHavePosition, IHavePrimaryKey;
    void CopyToReferers<TReferer, TReferer2>(TReferer referer, IEnumerable<TReferer2> referers2, bool scoped)
        where TReferer : IHavePosition, IHavePrimaryKey
        where TReferer2 : IHavePrimaryKey;

    // bool IsMatch<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
    //     where TReferer : IHavePrimaryKey;
    IQueryable<int> GetMatchingRefererIntIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey;
    IQueryable<long> GetMatchingRefererLongIds<TReferer>(IEnumerable<IGeoIndexCell> searchCells)
        where TReferer : IHavePrimaryKey;
}
