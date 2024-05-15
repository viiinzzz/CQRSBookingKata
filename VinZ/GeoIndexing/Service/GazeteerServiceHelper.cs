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

public static class GazeteerServiceHelper
{
    public static IEnumerable<TReferer> IncludeGeoIndex<TReferer>(this IEnumerable<TReferer> referers, double precisionMaxKm, IGazetteerService geo)
        where TReferer : IHavePrimaryKeyAndPosition
    {
        return geo.IncludeGeoIndex(referers, precisionMaxKm);
    }

    public static IHaveCollection<TReferer> IncludeGeoIndex<TReferer>(this IHaveCollection<TReferer> haveReferers, double precisionMaxKm, IGazetteerService geo)
        where TReferer : IHavePrimaryKeyAndPosition
    {
        haveReferers.Collection = geo.IncludeGeoIndex(haveReferers.Collection, precisionMaxKm);

        return haveReferers;
    }

    public static (int RefereTypeHash, long RefererHash) GetRefererHashes<TReferer>(this TReferer? referer)
        where TReferer : IHavePrimaryKey
    {
        string refererType;

        if (referer is GeoProxy proxy)
        {
            refererType = proxy.TypeFullName;
        }
        else
        {
            refererType = typeof(TReferer).FullName
                          ?? throw new ArgumentException("type must have FullName", nameof(TReferer));
        }

        var refererTypeHash = refererType.GetHashCode();

        long refererHash = default;

        if (referer != null)
        {
            //refererHash = (referer.PrimaryKey, refererTypeHash).GetHashCode();

            refererHash = HashHelper.GetHashCode64(
                BitConverter.GetBytes(referer.PrimaryKey),
                BitConverter.GetBytes(refererTypeHash));
        }

        return (refererTypeHash, refererHash);
    }


    public static GeoIndexCellDistance EarthArcDist(this IGeoIndexCell cell1, IGeoIndexCell cell2)
    {
        var id1 = cell1.S2CellIdSigned.ToS2CellId();
        var id2 = cell2.S2CellIdSigned.ToS2CellId();

        var levelMin = (byte)Math.Min(id1.Level, id2.Level);

        var worseTolerance = levelMin.EarthMaxKmForS2Level();
        var bestTolerance = levelMin.EarthMinKmForS2Level();

        var p1 = id1.ToPoint();
        var p2 = id2.ToPoint();

        var angle = new S1Angle(p1, p2);

        var km = angle.Radians * S2LatLng.EarthRadiusMeters / 1000;

        return new GeoIndexCellDistance(km, bestTolerance, worseTolerance);
    }
}