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
    public static S2CellId ToS2CellId(this long cellId)
    {
        var id = cellId.MapSignedToUnsigned<long, ulong>();

        return new S2CellId(id);
    }

    public static string ToS2Hex(this S2CellId cellId)
    {
        return $"s2:{cellId.Id:x16}";
    }
}
