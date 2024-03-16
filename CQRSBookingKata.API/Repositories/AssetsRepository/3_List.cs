
namespace CQRSBookingKata.API.Repositories;

public partial class AssetsRepository
{
    public IQueryable<Employee> Employees => back.Employees.AsQueryable();
    public IQueryable<Hotel> Hotels => back.Hotels.AsQueryable();
    public IQueryable<Room> Rooms(int hotelId) => back.Rooms.Where(hotel => hotel.HotelId == hotelId);
}