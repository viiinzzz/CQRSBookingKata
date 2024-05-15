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

using BookingKata.Sales;

namespace Booking.Sales.Services;

public static class OvernightStayHelper
{

    public static IEnumerable<Vacancy> StayUntil(this OvernightStay firstNight,
        OvernightStay lastNight, int personMaxCount,
        double latitude, double longitude,
        string hotelName, string cityName, int urid)
    {

        return firstNight.DayNum.RangeTo(lastNight.DayNum)

            .Select(dayNum => new Vacancy(dayNum, personMaxCount, latitude, longitude,
                hotelName, cityName, false, urid));
    }

    public static int[] DayNumssUntil(this OvernightStay firstNight,
        OvernightStay lastStay, int urid)
    {
        return firstNight.StayUntil(lastStay, 0,
                0, 0,
                string.Empty, string.Empty, urid)
            .Select(vacancy => vacancy.DayNum)
            .ToArray();
    }
    public static long[] VacancyIdsUntil(this OvernightStay firstNight,
        OvernightStay lastStay, int urid)
    {
        return firstNight.StayUntil(lastStay, 0,
                0, 0,
                string.Empty, string.Empty, urid)
            .Select(vacancy => vacancy.VacancyId)
            .ToArray();
    }
}