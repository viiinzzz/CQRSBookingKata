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

namespace VinZ.Common;


public static class PositionHelper
{
    public static double ArcDist(this Position p1, Position p2, double radius)
    {
        //Haversine

        var lat = p1.Latitude.DegToRad();
        var lon = p2.Latitude.DegToRad();
        var dLat = (p2.Latitude - p1.Latitude).DegToRad();
        var dLon = (p2.Longitude - p1.Longitude).DegToRad();

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat) * Math.Cos(lon);

        var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));

        var d = radius * c;

        return d;
    }

    public static double DegToRad(this double val)
    {
        return Math.PI / 180 * val;
    }
}
