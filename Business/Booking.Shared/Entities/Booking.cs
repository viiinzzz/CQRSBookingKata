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

using System.ComponentModel.DataAnnotations.Schema;

namespace BookingKata.Shared;

public record Booking
(
    DateTime ArrivalDate = default,
    DateTime DepartureDate = default,
    int ArrivalDayNum = default,
    int DepartureDayNum = default,
    int NightsCount = default,

    double Latitude = default,
    double Longitude = default,

    string HotelName = default,
    string CityName = default,

    string LastName = default,
    string FirstName = default,

    int PersonCount = default,

    double Price = default,
    string Currency = default,

    int RoomNum = default,
    int FloorNum = default,
    int HotelId = default,

    int UniqueRoomId = default,
    int CustomerId = default,
    int BookingId = default,
    bool Cancelled = false
)
    : RecordWithValidation, IHavePrimaryKeyAndPosition
{

    protected override void Validate()
    {
        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);

    }

    [System.Text.Json.Serialization.JsonIgnore]
    // [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    // [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

    public long PrimaryKey => BookingId;
}