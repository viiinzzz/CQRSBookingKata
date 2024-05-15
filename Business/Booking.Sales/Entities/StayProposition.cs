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

namespace BookingKata.Sales;

public record StayProposition
(

    int PersonCount = default,
    int NightsCount = default,

    string ArrivalDateUtc = default,
    int ArrivalDayNum = int.MaxValue,
    string DepartureDateUtc = default,
    int DepartureDayNum = int.MaxValue,

    double Price = default,
    string Currency = default,

    string OptionStartUtc = default,
    int OptionStartDayNum = int.MinValue,
    string OptionEndUtc = default,
    int OptionEndDayNum = int.MinValue,

    int Urid = default,
    int StayPropositionId = default
)
{
    public bool IsValid(DateTime now)
    {
        var optionStartsUtc = OptionStartUtc.DeserializeUniversal_ThrowIfNull(nameof(OptionStartUtc));
        var optionEndsUtc = OptionEndUtc.DeserializeUniversal_ThrowIfNull(nameof(OptionEndUtc));

        return now >= optionStartsUtc &&
               now < optionEndsUtc;
    }
}