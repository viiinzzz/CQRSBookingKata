namespace BookingKata.Infrastructure.Storage;

public partial class SalesRepository
{

    public IQueryable<Booking> Bookings

        => _sales.Bookings
            .AsNoTracking();

    public int AddBooking(Booking booking)
    {
        var entity = _sales.Bookings.Add(booking);
        _sales.SaveChanges();

        entity.State = EntityState.Detached;

        return booking.BookingId;
    }
}