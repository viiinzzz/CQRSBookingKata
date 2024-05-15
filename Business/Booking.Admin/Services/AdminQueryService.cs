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

namespace BookingKata.Admin;


public class AdminQueryService
(
    IAdminRepository admin,
    IGazetteerService geo
)
{
    public IQueryable<RoomDetails> GetHotelRoomDetails(int hotelId, int[]? exceptUridsOrRoomNums, int[]? onlyUridsOrRoomNums)
    {
        var exceptUrids = !(exceptUridsOrRoomNums is not { Length: > 0 } || exceptUridsOrRoomNums.Any(x => x < 1_0000)); 
        var exceptRoomNums = !(exceptUridsOrRoomNums is not { Length: > 0 } || exceptUridsOrRoomNums.Any(x => x >= 1_0000)); 

        var onlyUrids = !(onlyUridsOrRoomNums is not { Length: > 0 } || onlyUridsOrRoomNums.Any(x => x < 1_0000)); 
        var onlyRoomNums = !(onlyUridsOrRoomNums is not { Length: > 0 } || onlyUridsOrRoomNums.Any(x => x >= 1_0000));


        if (exceptUridsOrRoomNums is { Length: > 0 } && !exceptUrids && !exceptRoomNums)
        {
            throw new ArgumentException("invalid mix of urid, roomNum", nameof(exceptUridsOrRoomNums));
        }

        if (onlyUridsOrRoomNums is { Length: > 0 } && !onlyUrids && !onlyRoomNums)
        {
            throw new ArgumentException("invalid mix of urid, roomNum", nameof(onlyUridsOrRoomNums));
        }

        var hotel = admin.GetHotel(hotelId);

        if (hotel == default)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(hotelId));
        }

        if (hotel.Disabled)
        {
            throw new ArgumentException(ReferenceDisabled, nameof(hotelId));
        }

        var hotelCell = geo.RefererGeoIndex(hotel);

        if (hotelCell == null)
        {
            throw new ArgumentException(ReferenceNotIndexed, nameof(hotelId));
        }

        //sales search will be performed against known cities list,
        //hence determining nearest known city name for geo-indexing

        var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelCell, NearestKnownCityMaxKm);
        var nearestKnownCityName = nearestKnownCity?.name;

        var rooms = admin.Rooms(hotelId);

        var floorNumMax = new UniqueRoomId(rooms.Max(room => room.Urid)).FloorNum;

        if (exceptUrids) {
            rooms = rooms
                .Where(room => !exceptUridsOrRoomNums.Contains(room.Urid));
        }
        if (exceptRoomNums) {
            rooms = rooms
                .Where(room => !exceptUridsOrRoomNums.Contains(room.RoomNum));
        }

        if (onlyUrids)
        {
            rooms = rooms
                .Where(room => onlyUridsOrRoomNums.Contains(room.Urid));
        }
        if (onlyRoomNums)
        {
            rooms = rooms
                .Where(room => onlyUridsOrRoomNums.Contains(room.RoomNum));
        }

        var roomDetails = rooms
            .Select(room => new RoomDetails(
                room.PersonMaxCount,
                hotel.Latitude,
                hotel.Longitude,
                hotel.HotelName,
                hotel.ranking,
                nearestKnownCityName,
                hotel.EarliestCheckInHours,
                hotel.LatestCheckOutHours,
                new UniqueRoomId(room.Urid).FloorNum,
                floorNumMax,
                room.Urid
            ));

        return roomDetails;
    }




    public RoomDetails GetSingleRoomDetails(int urid)
    {
        var uniqueRoomId = new UniqueRoomId(urid);

        var hotelId = uniqueRoomId.HotelId;

        var hotel = admin.GetHotel(hotelId);

        if (hotel == default)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(hotelId));
        }

        if (hotel.Disabled)
        {
            throw new ArgumentException(ReferenceDisabled, nameof(hotelId));
        }

        var hotelCell = geo.RefererGeoIndex(hotel);

        if (hotelCell == null)
        {
            throw new ArgumentException(ReferenceNotIndexed, nameof(hotelId));
        }

        //sales search will be performed against known cities list,
        //hence determining nearest known city name for geo-indexing

        var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelCell, NearestKnownCityMaxKm);
        var nearestKnownCityName = nearestKnownCity?.name;

        var rooms = admin.Rooms(hotelId);

        var floorNumMax = new UniqueRoomId(rooms.Max(room => room.Urid)).FloorNum;

        var room = rooms.FirstOrDefault(room => room.RoomNum == uniqueRoomId.RoomNum);

        if (room == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(urid));
        }

        var roomDetails = new RoomDetails(
                room.PersonMaxCount,
                hotel.Latitude,
                hotel.Longitude,
                hotel.HotelName,
                hotel.ranking,
                nearestKnownCityName,
                hotel.EarliestCheckInHours,
                hotel.LatestCheckOutHours,
                uniqueRoomId.FloorNum,
                floorNumMax,
                room.Urid
            );

        return roomDetails;
    }



    public RoomDetails[] GetManyRoomDetails(int[] urids)
    {
        var roomDetails = urids
            .Select(x => new
            {
                urid = x,
                hotelId = new UniqueRoomId(x).HotelId
            })
            .GroupBy(x => x.hotelId)
            .SelectMany( x =>
            {
                var hotelId = x.Key;
                var only = x.Select(x => x.urid).ToArray();

                return GetHotelRoomDetails(hotelId, null, only);
            })
            .ToArray();
       
        return roomDetails;
    }

}