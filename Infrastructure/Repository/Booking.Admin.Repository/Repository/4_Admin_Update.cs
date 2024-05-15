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
    public Employee Update(int employeeId, UpdateEmployee update)
    {
        var employee = _admin.Employees
            .Find(employeeId);

        if (employee == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        _admin.Entry(employee).State = EntityState.Detached;

        employee = employee.Patch(update);

        var entity = _admin.Employees.Update(employee);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return employee;
    }


    public Hotel Update(int hotelId, ModifyHotel modify)
    {
        var hotel = _admin.Hotels
            .Find(hotelId);

        if (hotel == default)
        {
            throw new InvalidOperationException("hotelId not found");
        }

        _admin.Entry(hotel).State = EntityState.Detached;

        hotel = hotel.Patch(modify);

        var entity = _admin.Hotels.Update(hotel);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return hotel;
    }


    public Room Update(int urid, UpdateRoom update)
    {
        var room = _admin.Rooms
            .Find(urid);

        if (room == default)
        {
            throw new InvalidOperationException("urid not found");
        }

        _admin.Entry(room).State = EntityState.Detached;

        room = room.Patch(update);

        var entity = _admin.Rooms.Update(room);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return room;
    }

}