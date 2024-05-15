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

namespace BookingKata.Admin;

public record Hotel
(
    string HotelName,

    double Latitude,
    double Longitude,

    int EarliestCheckInTime = 16_00,
    int LatestCheckOutTime = 10_00,

    string LocationAddress = "",
    string ReceptionPhoneNumber = "",
    string url = "",
    int ranking = 2,

    int? ManagerId = default,

    int HotelId = default,
    bool Disabled = false
) 
    : RecordWithValidation, IHavePrimaryKeyAndPosition
{
    protected override void Validate()
    {
        if (CheckInH is < 0 or > 23)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(EarliestCheckInTime));
        }
        if (CheckOutH is < 0 or > 23)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(LatestCheckOutTime));
        }
        if (CheckInM is < 0 or > 59)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(EarliestCheckInTime));
        }
        if (CheckOutM is < 0 or > 59)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(LatestCheckOutTime));
        }

        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);

    }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long PrimaryKey => HotelId;

    private int CheckInH => EarliestCheckInTime / 100;
    private int CheckInM => EarliestCheckInTime % 100;
    private int CheckOutH => LatestCheckOutTime / 100;
    private int CheckOutM => LatestCheckOutTime % 100;


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore] 
    public double EarliestCheckInHours => CheckInH + CheckInM / 60d;

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore] 
    public double LatestCheckOutHours => CheckOutH + CheckOutM / 60d;


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