
namespace CQRSBookingKata.API;

public partial class AssetsRepository
{
    public IQueryable<Employee> Employees => _back.Employees.AsQueryable();
    public IQueryable<Hotel> Hotels => _back.Hotels.AsQueryable();
    public IQueryable<Room> Rooms(int hotelId) => _back.Rooms.Where(hotel => hotel.HotelId == hotelId);
}