/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.GeoIndexing;

public abstract partial class GazetteerServiceBase
{
    public GeoIndexCellDistance EarthArcDist(IGeoIndexCell cell1, IGeoIndexCell cell2)
        => cell1.EarthArcDist(cell2);

    public IGeoIndexCell[] NewGeoIndex<TSearch>(TSearch search, double maxPrecisionKm, double? maxDistanceKm)
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
    public IGeoIndexCell[] CacheGeoIndex<TEntity>(TEntity entity, double precisionMaxKm) 
        where TEntity : IHavePosition
    {
        var (minLevel, maxLevel) = S2GeometryHelper.S2MinMaxLevelForKm(precisionMaxKm, default);

        return GetCache<TEntity>().GetCache(entity)
            .Where(cell => 
                cell.S2Level <= maxLevel && 
                cell.S2Level >= minLevel)
            .Select(cell => (IGeoIndexCell)cell)

            .ToArray();
    }

    public IGeoIndexCell CacheGeoIndex<TEntity>(TEntity entity) 
        where TEntity : IHavePosition
    {
        return GetCache<TEntity>().GetCache(entity).MaxBy(c => c.S2Level);
    }


    private readonly ConcurrentDictionary<Type, GeoIndexCache> _cacheGeoIndex = new();
    
    public void ClearCacheGeoIndex()
    {
        _cacheGeoIndex.Clear();
    }

    private GeoIndexCache GetCache<TEntity>() 
        where TEntity : IHavePosition
    {
        if (!_cacheGeoIndex.TryGetValue(typeof(TEntity), out var cache))
        {
            return NewCache<TEntity>();
        }

        return cache;
    }

    private GeoIndexCache NewCache<TEntity>()
        where TEntity : IHavePosition
    {
        var cache = new GeoIndexCache();

        _cacheGeoIndex[typeof(TEntity)] = cache;

        return cache;
    }


    private class GeoIndexCache
    {
        private readonly ConcurrentDictionary<IHavePosition, GeoIndexCell[]> _cache = new();

        public void Clear() => _cache.Clear();

        public GeoIndexCell[] GetCache(IHavePosition entity)
        {
            if (!_cache.TryGetValue(entity, out var arr))
            {
                return NewCache(entity);
            }

            return arr;
        }

        private GeoIndexCell[] NewCache(IHavePosition entity)
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
