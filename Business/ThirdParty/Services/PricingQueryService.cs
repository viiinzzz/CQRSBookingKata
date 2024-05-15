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

using BookingKata.Shared;

namespace BookingKata.ThirdParty;

//sales and marketing, event planning

public class PricingQueryService
(

)
    : IPricingQueryService
{

    public Price GetPrice(
        //room
        int personMaxCount, int floorNum, int floorNumMax, int hotelRank, double latitude, double longitude,

        //booking
        int personCount, DateTime arrivalDate, DateTime departureDate, string? currency, CustomerProfile? customerProfile
        )
    {
        // var uniqueRoomId = new UniqueRoomId(urid);

        // var hotel = admin.GetHotel(uniqueRoomId.HotelId);
        // var room = admin.GetRoom(uniqueRoomId.Value); //check room category, extra
        // var personMaxCount = room.PersonMaxCount; //charge on headcount or capacity?
        // var floorNum = uniqueRoomId.FloorNum; //higher more expensive
        // var roomNum = uniqueRoomId.RoomNum;
        //supercharge on peak days special events vacation sport concert weekend
        //lower when load too low
        //user forecast system with news feed, customer history
        //customer may have discount

        //static pricing

        var nightsCount = OvernightStay.From(arrivalDate).To(departureDate);

        if (personMaxCount <= 1)
        {
            return new Price(145, "EUR"); //145/p
        }

        if (personMaxCount <= 2)
        {
            return new Price(230, "EUR"); //115/p
        }

        if (personMaxCount <= 3)
        {
            return new Price(290, "EUR"); //96/p, 
        }

        if (personMaxCount <= 4)
        {
            return new Price(320, "EUR"); //80/p, 
        }

        else //5p
        {
            return new Price(375, "EUR"); //75/p
        }


        //todo dynamic pricing
    }

}