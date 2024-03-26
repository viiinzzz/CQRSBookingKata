namespace VinZ.GeoIndexing;

public abstract partial class GazeteerServiceBase
{
    public GeoIndexCellDistance EarthArcDist(IGeoIndexCell cell1, IGeoIndexCell cell2)
        => cell1.EarthArcDist(cell2);

    public IGeoIndexCell[] SearchGeoIndex<TSearch>(TSearch search, double maxPrecisionKm, double? maxDistanceKm)
        where TSearch: IHavePosition
    {
        var (minLevel, maxLevel) = S2GeometryHelper.S2MinMaxLevelForKm(maxPrecisionKm, maxDistanceKm);

        var position = search.Position;

        if (position == null)
        {
            return Array.Empty<IGeoIndexCell>();
        }

        return position.Value
            .AllGeoIndexCell()
            .Where(cell => 
                cell.S2Level <= maxLevel && 
                cell.S2Level >= minLevel)
            .Select(cell => (IGeoIndexCell)cell)
            .ToArray();
    }
    public IGeoIndexCell[] GeoIndex<TEntity>(TEntity entity, double maxPrecisionKm) 
        where TEntity : IHavePosition
    {
        var (minLevel, maxLevel) = S2GeometryHelper.S2MinMaxLevelForKm(maxPrecisionKm, default);

        return Get<TEntity>().Get(entity)
            .Where(cell => 
                cell.S2Level <= maxLevel && 
                cell.S2Level >= minLevel)
            .Select(cell => (IGeoIndexCell)cell)

            .ToArray();
    }

    public IGeoIndexCell GeoIndex<TEntity>(TEntity entity) 
        where TEntity : IHavePosition
    {
        return Get<TEntity>().Get(entity).Last();
    }


    private readonly ConcurrentDictionary<Type, CacheGeoIndex> _cacheGeoIndex = new();
    
    public void ClearCacheGeoIndex()
    {
        _cacheGeoIndex.Clear();
    }

    private CacheGeoIndex Get<TEntity>() 
        where TEntity : IHavePosition
    {
        if (!_cacheGeoIndex.TryGetValue(typeof(TEntity), out var cache))
        {
            return Cache<TEntity>();
        }

        return cache;
    }

    private CacheGeoIndex Cache<TEntity>()
        where TEntity : IHavePosition
    {
        var cache = new CacheGeoIndex();

        _cacheGeoIndex[typeof(TEntity)] = cache;

        return cache;
    }


    private class CacheGeoIndex
    {
        private readonly ConcurrentDictionary<IHavePosition, GeoIndexCell[]> _cache = new();

        public void Clear() => _cache.Clear();

        public GeoIndexCell[] Get(IHavePosition entity)
        {
            if (!_cache.TryGetValue(entity, out var arr))
            {
                return Cache(entity);
            }

            return arr;
        }

        private GeoIndexCell[] Cache(IHavePosition entity)
        {
            if (!entity.Position.HasValue)
            {
                return Array.Empty<GeoIndexCell>();
            }

            var arr = entity.Position.Value
                .AllGeoIndexCell()
                .ToArray();

            _cache[entity] = arr;

            return arr;
        }
    }

}
