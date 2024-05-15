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

public interface IGazetteerService
{
    IQueryable<IGeoIndex> Indexes { get; }

    TReferer IncludeGeoIndex<TReferer>(TReferer referer, byte maxLevel)
        where TReferer : IHavePrimaryKeyAndPosition;

    IEnumerable<TReferer> IncludeGeoIndex<TReferer>(IEnumerable<TReferer> referers, double precisionMaxKm)
        where TReferer : IHavePrimaryKeyAndPosition;


    IQueryable<City> QueryCities(string cityName, bool? approximateNameMatch, string? countryCode);
    (City?, double) NearestCity(IGeoIndexCell searchCell, double maxKm);

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
        where TReferer : IHavePrimaryKeyAndPosition
        where TReferer2 : IHavePrimaryKey; 
    void CopyToReferer<TReferer, TReferer2>(TReferer referer, TReferer2 referer2)
        where TReferer : IHavePrimaryKeyAndPosition
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
