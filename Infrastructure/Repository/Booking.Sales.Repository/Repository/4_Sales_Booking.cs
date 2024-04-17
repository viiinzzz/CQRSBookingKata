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