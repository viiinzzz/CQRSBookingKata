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

namespace BookingKata.Sales;

//room vacancy

public record Vacancy
(
    int DayNum,
    int PersonMaxCount,

    double Latitude,
    double Longitude,

    string HotelName,
    string? CityName,

    bool Cancelled = false,
    int UniqueRoomId = default
)
    : RecordWithValidation, IHavePrimaryKeyAndPosition
{
    public long VacancyId { get; set; } = default;
    
    
    protected override void Validate()
    {
        if (UniqueRoomId is < 1 or >= 1_0000_0000 or 1_0000)
        {
            throw new ArgumentException("value must be at least 1 and at most 9999 9999, not 1 0000", nameof(UniqueRoomId));
        }
        if (DayNum is < 0 or >= 1_0000)
        {
            throw new ArgumentException("value must be at least 0 and at most 9999", nameof(DayNum));
        }
        if (PersonMaxCount is < 0 or >= 10)
        {
            throw new ArgumentException("value must be at least 0 and at most 9", nameof(PersonMaxCount));
        }
    
        if (VacancyId == 0)
        {
            // VacancyId = 
            //     UniqueRoomId * 1_0000 +
            //     DayNum; 
            VacancyId = 
                UniqueRoomId +
                DayNum * 1_0000_0000l;
        }

        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);

    }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long PrimaryKey => VacancyId;


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public Position? Position { get; private set; }
    

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

}