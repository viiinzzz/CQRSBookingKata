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

public record StayRequest
(
    int PersonCount,
    DateTime ArrivalDate,
    DateTime DepartureDate,

    int? ArrivalFlexBeforeDays = default,
    int? ArrivalFlexAfterDays = default,
    int? DepartureFlexBeforeDays = default,
    int? DepartureFlexAfterDays = default,
    int? NightsCountMin = default,
    int? NightsCountMax = default,

    bool? ApproximateNameMatch = default,
    string? HotelName = default,
    string? CountryCode = default,
    string? CityName = default,

    double? Latitude = default,
    double? Longitude = default,
    int? MaxKm = default,

    double? PriceMin = default,
    double? PriceMax = default,
    string? Currency = default
) : RecordWithValidation, IHavePosition
{
    public const int NightsCountMaxAllowed = 90;

    public int Nights => (int) (DepartureDate.DayStart() - ArrivalDate.DayStart()).TotalDays;

    protected override void Validate()
    {
        var coordinatesCount = 0;
        if (Latitude.HasValue) coordinatesCount++;
        if (Longitude.HasValue) coordinatesCount++;

        switch (coordinatesCount)
        {
            case 1:
            {
                var argName = string.Empty;
                if (!Latitude.HasValue) argName = nameof(Latitude);
                if (!Longitude.HasValue) argName = nameof(Longitude);

                throw new ArgumentException($"must specify both or none of {nameof(Latitude)}, {nameof(Longitude)}",
                    argName);
            }
            case 2:
                Position = new Position(Latitude!.Value, Longitude!.Value);
                break;
        }

        if (ArrivalDate > DepartureDate)
        {
            throw new ArgumentException($"must be later than {nameof(ArrivalDate)}", nameof(DepartureDate));
        }

        var nights = Nights;

        if (nights < 1)
        {
            throw new ArgumentException($"the stay must span over one night or more", nameof(DepartureDate));
        }

        if (nights >= NightsCountMaxAllowed)
        {
            throw new ArgumentException($"the stay must span over no more than {NightsCountMaxAllowed} nights", nameof(DepartureDate));
        }


        if (NightsCountMin.HasValue && (NightsCountMin < 1 || NightsCountMin > nights))
        {
            throw new ArgumentException($"must be null or one or greater but no more than {nights}", nameof(NightsCountMin));
        }

        if (NightsCountMax.HasValue && (NightsCountMax < 1 || NightsCountMax > nights))
        {
            throw new ArgumentException($"must be null or one or greater but no more than {nights}", nameof(NightsCountMin));
        }

        if (NightsCountMin.HasValue && NightsCountMax.HasValue && NightsCountMax < NightsCountMin)
        {
            throw new ArgumentException($"must be equal or greater than {nameof(NightsCountMin)}", nameof(NightsCountMax));
        }

        if (ArrivalDate.Year == DepartureDate.Year && ArrivalDate.Month == DepartureDate.Month && ArrivalDate.Day == DepartureDate.Day)
        {
            throw new ArgumentException($"must be different of {nameof(ArrivalDate)}", nameof(DepartureDate));
        }

        if (ArrivalFlexBeforeDays is < 0 or >= NightsCountMaxAllowed)
        {
            throw new ArgumentException($"must be zero or greater but no more than {NightsCountMaxAllowed}", nameof(ArrivalFlexBeforeDays));
        }

        if (ArrivalFlexAfterDays is < 0 or >= NightsCountMaxAllowed)
        {
            throw new ArgumentException($"must be zero or greater but no more than  {NightsCountMaxAllowed}", nameof(ArrivalFlexAfterDays));
        }

        if (DepartureFlexBeforeDays is < 0 or >= NightsCountMaxAllowed)
        {
            throw new ArgumentException($"must be zero or greater but no more than  {NightsCountMaxAllowed}", nameof(DepartureFlexBeforeDays));
        }

        if (DepartureFlexAfterDays is < 0 or >= NightsCountMaxAllowed)
        {
            throw new ArgumentException($"must be zero or greater but no more than {NightsCountMaxAllowed}", nameof(DepartureFlexAfterDays));
        }

        var nightsFlexAfter = ArrivalFlexAfterDays.HasValue ? nights - ArrivalFlexAfterDays : null;
        if (nightsFlexAfter.HasValue && (nightsFlexAfter < 1 || (NightsCountMin.HasValue && nightsFlexAfter < NightsCountMin))) 
        {
            throw new ArgumentException($"must be less than stay period", nameof(ArrivalFlexAfterDays));
        }

        var nightsFlexBefore = DepartureFlexBeforeDays.HasValue ?  nights - DepartureFlexBeforeDays : null;
        if (nightsFlexBefore.HasValue && (nightsFlexBefore < 1 || (NightsCountMin.HasValue && nightsFlexAfter < NightsCountMin)))
        {
            throw new ArgumentException($"must be less than stay period", nameof(DepartureFlexBeforeDays));
        }

        if (PersonCount is < 1 or > 9)
        {
            throw new ArgumentException("must be 1 or greater and at most 9", nameof(PersonCount));
        }
    }


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