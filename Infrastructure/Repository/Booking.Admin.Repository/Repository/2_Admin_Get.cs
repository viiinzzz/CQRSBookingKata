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

namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository
{
    public Hotel? GetHotel(int hotelId)
    {
        var hotel = _admin.Hotels.Find(hotelId);

        if (hotel == default) return default;

        _admin.Entry(hotel).State = EntityState.Detached;

        var hotelCells = geo.CacheGeoIndex(hotel, bconf.PrecisionMaxKm);

        hotel.Cells = hotelCells;

        return hotel;
    }

    public Room? GetRoom(int uniqueRoomId)
    {
        var room = _admin.Rooms
            .Find(uniqueRoomId);

        if (room == default) return default;

        _admin.Entry(room).State = EntityState.Detached;

        return room;
    }

    public Employee? GetEmployee(int employeeId)
    {
        var employee = _admin.Employees
            .Find(employeeId);

        if (employee == default) return default;

        _admin.Entry(employee).State = EntityState.Detached;

        return employee;
    }

}