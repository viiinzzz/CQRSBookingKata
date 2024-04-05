namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository
{
    public void Update(int employeeId, UpdateEmployee update)
    {
        var employee = _admin.Employees
            .Find(employeeId);

        if (employee == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        _admin.Entry(employee).State = EntityState.Detached;

        employee = employee.Patch(update);

        var entity = _admin.Employees.Update(employee);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;
    }


    public void Update(int hotelId, UpdateHotel update)
    {
        var hotel = _admin.Hotels
            .Find(hotelId);

        if (hotel == default)
        {
            throw new InvalidOperationException("hotelId not found");
        }

        _admin.Entry(hotel).State = EntityState.Detached;

        hotel = hotel.Patch(update);

        var entity = _admin.Hotels.Update(hotel);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;
    }


    public void Update(int urid, UpdateRoom update)
    {
        var room = _admin.Rooms
            .Find(urid);

        if (room == default)
        {
            throw new InvalidOperationException("urid not found");
        }

        _admin.Entry(room).State = EntityState.Detached;

        room = room.Patch(update);

        var entity = _admin.Rooms.Update(room);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;
    }

}