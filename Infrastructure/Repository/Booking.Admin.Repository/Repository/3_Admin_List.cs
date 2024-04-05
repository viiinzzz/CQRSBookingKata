namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository
{
    public IQueryable<Employee> Employees
        
        => _admin.Employees
            .AsNoTracking();

    public IQueryable<Hotel> Hotels 
        
        => _admin.Hotels
            .AsNoTracking();

    public IQueryable<Room> Rooms(int hotelId)

        => _admin.Rooms
            .AsNoTracking()
            .Where(room => room.HotelId == hotelId);
}