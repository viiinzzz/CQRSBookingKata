
namespace CQRSBookingKata.API;

public partial class AdminRepository
{
    public IQueryable<Employee> Employees
        
        => _admin.Employees
            .AsNoTracking();

    public IQueryable<Hotel> Hotels 
        
        => _admin.Hotels
            .Include(hotel => hotel.Cells)
            .AsNoTracking();

    public IQueryable<Room> Rooms(int hotelId)

        => _admin.Rooms
            .AsNoTracking()
            .Where(room => room.HotelId == hotelId);

    public IQueryable<Booking> Bookings

        => _admin.Bookings
            .AsNoTracking();

}