namespace BookingKata.API;

public partial class AdminRepository
{
    public void DisableHotel(int hotelId, bool disable, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var hotel = GetHotel(hotelId);

            if (hotel == default)
            {
                throw new InvalidOperationException("hotelId not found");
            }

            var update = hotel with
            {
                Disabled = disable
            };

            var entity = _admin.Hotels.Update(update);
            _admin.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public void DeleteRoom(int roomId, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var found = _admin.Rooms
                .Find(roomId);

            if (found == default)
            {
                throw new InvalidOperationException("roomId not found");
            }

            var entity = _admin.Rooms.Remove(found);
            _admin.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }

    }

    public void DisableEmployee(int employeeId, bool disable, bool scoped)
    {
        try 
        {
            using var scope = !scoped ? null : new TransactionScope();
            var employee = _admin.Employees.Find(employeeId);

            if (employee == default)
            {
                throw new InvalidOperationException("employeeId not found");
            }

            var update = employee with
            {
                Disabled = disable
            };

            var entity = _admin.Employees.Update(update);
            _admin.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }
}