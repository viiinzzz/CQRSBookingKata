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

public partial class SalesQueryService
{
    public long[] OpenBooking(UniqueRoomId urid, DateTime openingDate, DateTime closingDate, int personCount)
    {
        var originator = GetType().FullName
                         ?? throw new Exception("invalid originator");

        var roomDetails = bus.AskResult<RoomDetails>(Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new Id<RoomRef>(urid.Value), originator);

        if (roomDetails == default)
        {
            new ArgumentException(ReferenceInvalid, nameof(urid));
        }


        var hotelGeoProxy = bus.AskResult<GeoProxy>(Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id<HotelRef>(urid.HotelId), originator);

        if (hotelGeoProxy?.Cells is null or { Count: 0 })
        {
            throw new HotelNotGeoIndexedException();
        }

        var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelGeoProxy.Cells.TopCell(), NearestKnownCityMaxKm);


        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var vacancies = firstNight
            .StayUntil(lastNight, personCount, roomDetails.Latitude, roomDetails.Longitude,
                roomDetails.HotelName, nearestKnownCity?.name, urid.Value)
            .ToArray();

        geo.CopyToReferers(hotelGeoProxy, vacancies);

        //
        //
        sales.AddVacancies(vacancies);
        //
        //

        return vacancies
            .Select(vacancy => vacancy.VacancyId)
            .ToArray();
    }
}