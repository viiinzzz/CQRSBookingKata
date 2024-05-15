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

public static partial class S2GeometryHelper
{
    public static IEnumerable<GeoIndex> GetGeoIndexes<TReferer>(TReferer referer, double? minKm, double? maxKm) 
        where TReferer : IHavePrimaryKeyAndPosition
    {
        var (refererTypeHash, refererHash) = referer.GetRefererHashes();

        var p = referer.Position;

        if (!p.HasValue)
        {
            return Enumerable.Empty<GeoIndex>();
        }

        var range = new RangeGeoIndex(p.Value, minKm, maxKm);

        return range
            .AsCellIds()
            .Select(c => new GeoIndex(c.S2CellIdSigned, c.S2Level, referer.PrimaryKey, refererTypeHash, refererHash));
    }
}
