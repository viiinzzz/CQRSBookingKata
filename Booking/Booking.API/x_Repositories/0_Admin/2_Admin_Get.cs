
namespace BookingKata.API;

public partial class AdminRepository
{
    public Hotel? GetHotel(int hotelId)
    {
        var hotel = _admin.Hotels.Find(hotelId);

        _admin.Entry<Hotel>(hotel).State = EntityState.Detached;

        var hotelCells = geo.CacheGeoIndex(hotel, SalesQueryService.PrecisionMaxKm);

        hotel.Cells = hotelCells;

        return hotel;
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