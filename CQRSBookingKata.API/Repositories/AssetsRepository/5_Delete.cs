
namespace CQRSBookingKata.API.Repositories;

public partial class AssetsRepository
{
    public void DisableHotel(int hotelId, bool disable)
    {
        using var transaction = back.Database.BeginTransaction();

        try
        {
            var hotel = GetHotel(hotelId);

            if (hotel == default)
            {
                throw new InvalidOperationException("hotelId not found");
            }

            back.Hotels.Update(hotel with
            {
                Disabled = disable
            });

            back.SaveChanges();
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
        var found = back.Rooms.Find(roomId);

        if (found == default)
        {
            throw new InvalidOperationException("roomId not found");
        }

        back.Rooms.Remove(found);

        back.SaveChanges();
    }

    public void DisableEmployee(int employeeId, bool disable)
    {
        var employee = back.Employees.Find(employeeId);

        if (employee == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        back.Employees.Update(employee with
        {
            Disabled = disable
        });

        back.SaveChanges();
    }
}