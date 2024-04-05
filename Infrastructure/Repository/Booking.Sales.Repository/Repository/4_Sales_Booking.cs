namespace BookingKata.Infrastructure.Storage;

public partial class SalesRepository
{

    public IQueryable<Sales.Booking> Bookings

        => _sales.Bookings
            .AsNoTracking();

    public int AddBooking(Sales.Booking booking)
    {
        var entity = _sales.Bookings.Add(booking);
        _sales.SaveChanges();

        entity.State = EntityState.Detached;

        return booking.BookingId;
    }
}