
namespace CQRSBookingKata.API;

public partial class AssetsRepository
{
    public void DisableHotel(int hotelId, bool disable)
    {
        using var transaction = _back.Database.BeginTransaction();

        try
        {
            var hotel = GetHotel(hotelId);

            if (hotel == default)
            {
                throw new InvalidOperationException("hotelId not found");
            }

            _back.Hotels.Update(hotel with
            {
                Disabled = disable
            });

            _back.SaveChanges();
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }

    public void DeleteRoom(int roomId)
    {
        var found = _back.Rooms.Find(roomId);

        if (found == default)
        {
            throw new InvalidOperationException("roomId not found");
        }

        _back.Rooms.Remove(found);

        _back.SaveChanges();
    }

    public void DisableEmployee(int employeeId, bool disable)
    {
        var employee = _back.Employees.Find(employeeId);

        if (employee == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        _back.Employees.Update(employee with
        {
            Disabled = disable
        });

        _back.SaveChanges();
    }
}