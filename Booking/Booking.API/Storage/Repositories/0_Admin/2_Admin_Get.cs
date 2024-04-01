
using BookingKata.Planning;

namespace BookingKata.API;

public partial class AdminRepository
{
    public Hotel? GetHotel(int hotelId)
    {
        var hotel = _admin.Hotels.Find(hotelId);

        if (hotel == default) return default;

        _admin.Entry(hotel).State = EntityState.Detached;

        var hotelCells = geo.CacheGeoIndex(hotel, SalesQueryService.PrecisionMaxKm);

        hotel.Cells = hotelCells;

        return hotel;
    }

    public Room? GetRoom(int uniqueRoomId)
    {
        var room = _admin.Rooms
            .Find(uniqueRoomId);

        if (room == default) return default;

        _admin.Entry(room).State = EntityState.Detached;

        return room;
    }

    public Employee? GetEmployee(int employeeId)
    {
        var employee = _admin.Employees
            .Find(employeeId);

        if (employee == default) return default;

        _admin.Entry(employee).State = EntityState.Detached;

        return employee;
    }

}