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

//housekeeping, guest services, food and beverage service, security, IT, maintenance, HR

public interface IAdminRepository
{
    IQueryable<Employee> Employees { get; }
    int Create(CreateEmployeeRequest employee);
    Employee? GetEmployee(int employeeId);
    Employee Update(int employeeId, UpdateEmployee update);
    Employee DisableEmployee(int employeeId, bool disable);


    IQueryable<Hotel> Hotels { get; }
    int Create(NewHotel hotel);
    Hotel? GetHotel(int hotelId);
    Hotel Update(int hotelId, ModifyHotel modify);
    Hotel DisableHotel(int hotelId, bool disable);


    IQueryable<Room> Rooms(int hotelId);
    int Create(Room room);
    Room? GetRoom(int uniqueRoomId);
    Room Update(int roomId, UpdateRoom update);
    void DeleteRoom(int roomId);
    int[] Create(CreateHotelFloor newRooms);
    int[] GetFloorNextRoomNumbers(int hotelId, int floorNum, int roomCount);
}