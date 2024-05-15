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

public record City
(
    string? name = default, 
    string? lat = default,
    string? lng = default,
    string? country = default,
    string? admin1 = default,
    string? admin2 = default
)
    : RecordWithValidation, IHavePosition
{
    private double? GetLatitude()
    {
        if (!double.TryParse(lat, out var value)
            || value > 90 || value < -90)
        {
            return default;
        }

        return value;
    }

    private double? GetLongitude()
    {
        if (!double.TryParse(lng, out var value)
            || value > 180 || value < -180)
        {
            return default;
        }

        return value;
    }

    private Position? GetPosition()
    {
        var latitude = GetLatitude();
        if (!latitude.HasValue)
        {
            return default;
        }

        var longitude = GetLongitude();
        if (!longitude.HasValue)
        {
            return default;
        }
        
        return new Position(latitude.Value, longitude.Value);
    }

    protected override void Validate()
    {
        Position = GetPosition();
    }


    [System.Text.Json.Serialization.JsonIgnore]
    // [Newtonsoft.Json.JsonIgnore]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

}
