
namespace CQRSBookingKata.API;

public partial class AssetsRepository
{
    public void Update(int employeeId, UpdateEmployee update)
    {
        var employee = _back.Employees.Find(employeeId);

        if (employee == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        employee = employee.Patch(update);

        _back.Employees.Update(employee);
        _back.SaveChanges();
    }


    public void Update(int hotelId, UpdateHotel update)
    {
        var hotel = _back.Hotels.Find(hotelId);

        if (hotel == default)
        {
            throw new InvalidOperationException("hotelId not found");
        }

        hotel = hotel.Patch(update);

        _back.Hotels.Update(hotel);
        _back.SaveChanges();
    }


    public void Update(int urid, UpdateRoom update)
    {
        var room = _back.Rooms.Find(urid);

        if (room == default)
        {
            throw new InvalidOperationException("urid not found");
        }

        room = room.Patch(update);

        _back.Rooms.Update(room);
        _back.SaveChanges();
    }

}