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

using Business.Common;

namespace BookingKata.Infrastructure.Storage;

public partial class SalesRepository
{
   

    public IQueryable<StayProposition> Propositions

        => _sales.Propositions
            .AsNoTracking();


    public void AddStayProposition(StayProposition proposition)
    {
        var entity = _sales.Propositions.Add(proposition);
        _sales.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public bool HasActiveProposition(DateTime now, int urid, DateTime arrivalDate, DateTime departureDate)
    {
        var nowDayNum = OvernightStay.From(now).DayNum;
        var arrivalDayNum = OvernightStay.From(arrivalDate).DayNum;
        var departureDayNum = OvernightStay.From(departureDate).DayNum;

        return _sales.Propositions
            .AsNoTracking()
            .Any(prop =>
                prop.Urid == urid &&

                nowDayNum >= prop.OptionStartDayNum &&
                nowDayNum < prop.OptionEndDayNum &&
                (
                    (prop.ArrivalDayNum >= arrivalDayNum && prop.DepartureDayNum <= departureDayNum) ||
                    (prop.ArrivalDayNum <= arrivalDayNum && prop.DepartureDayNum >= departureDayNum) ||
                    (prop.ArrivalDayNum <= arrivalDayNum && prop.DepartureDayNum >= arrivalDayNum) ||
                    (prop.ArrivalDayNum <= departureDayNum && prop.DepartureDayNum >= departureDayNum)
                )
            );
    }
}