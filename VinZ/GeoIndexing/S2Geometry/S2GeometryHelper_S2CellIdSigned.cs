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
    public static GeoIndexCell GeoIndexCell(this S2CellId cid) => new GeoIndexCell
    (
        S2CellIdSigned: cid.Id.MapUnsignedToSigned<ulong, long>(),
        S2Level: (byte)cid.Level
    );


    public static GeoIndexCell GeoIndexCell(this Position p) => GeoIndexCell
    (
        cid: new S2Cell(
            S2LatLng.FromDegrees(p.Latitude, p.Longitude)
            ).Id
    );


    public static S2CellId S2CellId(this GeoIndexCell cids) => cids.S2CellIdSigned.ToS2CellId();


    public static IEnumerable<GeoIndexCell> AllGeoIndexCell(this Position p)
    {
        var l = S2LatLng.FromDegrees(p.Latitude, p.Longitude);
        var c = new S2Cell(l);
        S2CellId? cid = default;

        while (true)
        {
            cid = cid?.Parent ?? c.Id;

            var id = GeoIndexCell(cid.Value);

            yield return id;

            if (id.S2Level <= 0) yield break;
        }
    }
}
