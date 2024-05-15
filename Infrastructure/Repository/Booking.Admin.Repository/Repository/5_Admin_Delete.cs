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
    public Hotel DisableHotel(int hotelId, bool disable)
    {
        var hotel = GetHotel(hotelId);

        if (hotel == default)
        {
            throw new InvalidOperationException("hotelId not found");
        }

        var update = hotel with
        {
            Disabled = disable
        };

        var entity = _admin.Hotels.Update(update);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return update;
    }

    public Employee DisableEmployee(int employeeId, bool disable)
    {
        var employee = _admin.Employees.Find(employeeId);

        if (employee == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        var update = employee with
        {
            Disabled = disable
        };

        var entity = _admin.Employees.Update(update);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return update;
    }

    public void DeleteRoom(int roomId)
    {
        var found = _admin.Rooms
            .Find(roomId);

        if (found == default)
        {
            throw new InvalidOperationException("roomId not found");
        }

        var entity = _admin.Rooms.Remove(found);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;
    }

}