
namespace CQRSBookingKata.API.Repositories;

public partial class AssetsRepository
{
    public Hotel? GetHotel(int hotelId) => back.Hotels.Find(hotelId);

    public Room? GetRoom(int uniqueRoomId) => back.Rooms.Find(uniqueRoomId);

    public Employee? GetEmployee(int employeeId) => back.Employees.Find(employeeId);
}