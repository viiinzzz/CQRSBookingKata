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

public partial class BookingCommandService
{
    public void OpenHotelSeason(int hotelId, int[]? exceptRoomNumbers, DateTime openingDate, DateTime closingDate)
    {
        var originator = this.GetType().FullName;


        var hotelGeoProxy = bus.AskResult<GeoProxy>(Recipient.Admin, Verb.Admin.RequestFetchHotelGeoProxy,
            new Id(hotelId), originator);

        if (hotelGeoProxy == null)
        {
            throw new HotelNotFoundException();
        }


        var roomDetails = bus.AskResult<RoomDetails[]>(Recipient.Admin, Verb.Admin.RequestHotelRoomDetails,
            new RoomDetailsRequest(hotelId, exceptRoomNumbers), originator);

        if (roomDetails is null or { Length: 0 })
        {
            throw new RoomNotFoundException();
        }


        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var dayNumbers = firstNight.DayNum.RangeTo(lastNight.DayNum);


        var vacancies = roomDetails
            .SelectMany(roomDetail => dayNumbers
                .Select(dayNum => new Vacancy(dayNum,
                    roomDetail.PersonMaxCount, roomDetail.Latitude, roomDetail.Longitude,
                    roomDetail.HotelName, roomDetail.NearestKnownCityName, false, roomDetail.Urid)
                )
            ).ToList();


        geo.CopyToReferers(hotelGeoProxy, vacancies);

        sales.AddVacancies(vacancies);
    }
}