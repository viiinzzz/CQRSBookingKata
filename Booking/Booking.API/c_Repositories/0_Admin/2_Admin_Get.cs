
namespace CQRSBookingKata.API;

public partial class AdminRepository
{
    public Hotel? GetHotel(int hotelId)
    {
        var ret = _admin.Hotels
            .Include(hotel => hotel.Cells
                .OrderByDescending(cell => cell.S2Level))
            .FirstOrDefault(hotel => hotel.HotelId == hotelId);

        _admin.Entry<Hotel>(ret).State = EntityState.Detached;
       
        return ret;
    }

    public Room? GetRoom(int uniqueRoomId)
    {
        var ret = _admin.Rooms
            .Find(uniqueRoomId);

        _admin.Entry<Room>(ret).State = EntityState.Detached;

        return ret;
    }

    public Employee? GetEmployee(int employeeId)
    {
        var ret = _admin.Employees
            .Find(employeeId);

        _admin.Entry<Employee>(ret).State = EntityState.Detached;

        return ret;
    }

}