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

public record Room(
    int Urid,

    int HotelId,
    int RoomNum,
    int FloorNum,

    int PersonMaxCount
)
    : RecordWithValidation
{
    protected override void Validate()
    {
        var urid = new UniqueRoomId(Urid);

        if (urid.HotelId != HotelId || urid.RoomNum != RoomNum || urid.FloorNum != FloorNum)
        {
            throw new ArgumentException("value is not consistent", nameof(Urid));
        }

        if (PersonMaxCount is < 0 or > 5)
        {
            throw new ArgumentException("value must be at least 0 and at most 5", nameof(PersonMaxCount));
        }
    }
}