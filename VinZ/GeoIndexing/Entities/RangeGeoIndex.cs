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

public class RangeGeoIndex
{
    private readonly long?[] _indexes = new long?[31];

    public RangeGeoIndex(Position position, double? minKm, double? maxKm)
    {
        var (minLevel, maxLevel) = S2GeometryHelper.S2MinMaxLevelForKm(minKm, maxKm);

        foreach (var cid in position.AllGeoIndexCell())
        {
            if (cid.S2Level >= minLevel && cid.S2Level <= maxLevel)
            {
                _indexes[cid.S2Level] = cid.S2CellIdSigned;
            }
        }
    }

    public IEnumerable<GeoIndexCell> AsCellIds()
        => _indexes
            .Where(cellId => cellId.HasValue)
            .Select((cellId, level) => new GeoIndexCell(cellId!.Value, (byte)level));
}
