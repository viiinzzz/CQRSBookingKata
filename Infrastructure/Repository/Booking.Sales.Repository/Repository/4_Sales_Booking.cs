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

public partial class SalesRepository
{

    public IQueryable<Shared.Booking> Bookings

        => _sales.Bookings
            .AsNoTracking();

    public int AddBooking(Shared.Booking booking)
    {
        var entity = _sales.Bookings.Add(booking);
        _sales.SaveChanges();

        entity.State = EntityState.Detached;

        return booking.BookingId;
    }

    public Shared.Booking CancelBooking(int bookingId)
    {
        var booking = _sales.Bookings
            .Find(bookingId);

        if (booking == default)
        {
            throw new InvalidOperationException("bookingId not found");
        }

        if (booking.Cancelled)
        {
            throw new InvalidOperationException("already cancelled");
        }

        var cancelDate = DateTime.UtcNow;

        if (cancelDate >= booking.ArrivalDate)
        {
            throw new InvalidOperationException("booking already started or finished");
        }

        _sales.Entry(booking).State = EntityState.Detached;

        booking = booking with { Cancelled = true };

        var entity = _sales.Bookings.Update(booking);
        _sales.SaveChanges();
        entity.State = EntityState.Detached;

        return entity.Entity;
    }
}