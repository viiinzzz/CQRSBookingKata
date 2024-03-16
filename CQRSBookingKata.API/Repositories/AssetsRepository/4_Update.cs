
namespace CQRSBookingKata.API.Repositories;

public partial class AssetsRepository
{
    public void Update(int employeeId, UpdateEmployee update)
    {
        var employee = back.Employees.Find(employeeId);

        if (employee == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        employee = employee.Patch(update);

        back.Employees.Update(employee);
        back.SaveChanges();
    }


    public void Update(int hotelId, UpdateHotel update)
    {
        var hotel = back.Hotels.Find(hotelId);

        if (hotel == default)
        {
            throw new InvalidOperationException("hotelId not found");
        }

        hotel = hotel.Patch(update);

        back.Hotels.Update(hotel);
        back.SaveChanges();
    }


    public void Update(int urid, UpdateRoom update)
    {
        var room = back.Rooms.Find(urid);

        if (room == default)
        {
            throw new InvalidOperationException("urid not found");
        }

        room = room.Patch(update);

        back.Rooms.Update(room);
        back.SaveChanges();
    }

}