
using CQRSBookingKata.Assets;
using CQRSBookingKata.API.Databases;
using System.Globalization;
using CQRSBookingKata.Sales;

namespace CQRSBookingKata.API.Repositories;

public class AssetsRepository(BookingBackContext back) : IAssetsRepository
{
    public int CreateHotel(string hotelName, double latitude, double longitude)
    {
        var alreadyExist = FindHotel(hotelName, false);

        if (alreadyExist != default)
        {
            throw new InvalidOperationException("hotelName already exists");
        }

        var newtelly = new Hotel(hotelName, latitude, longitude);

        back.Hotels.Add(newtelly);

        back.SaveChanges();

        return newtelly.HotelId;
    }

    public int? FindHotel(string hotelName, bool approx)
    {
        var exactMatch = back.Hotels

            .FirstOrDefault(hotel => 0 == string.Compare(
                hotel.HotelName,
                hotelName,
                CultureInfo.CurrentCulture,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols))

            ?.HotelId;

        if (exactMatch != default)
        {
            return exactMatch;
        }

        if (!approx)
        {
            return default;
        }

        var approxMatch = back.Hotels

            .FirstOrDefault(hotel => -1 != CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                hotel.HotelName,
                hotelName,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols))

            ?.HotelId;

        return approxMatch;
    }

    public Hotel? GetHotel(int hotelId)
    {
        return back.Hotels
            
            .Find(hotelId);
    }

    public void UpdateHotel(Hotel hotel)
    {
        using var transaction = back.Database.BeginTransaction();

        try
        {
            var hotel2Id = FindHotel(hotel.HotelName, false);

            if (hotel2Id != hotel.HotelId)
            {
                throw new InvalidOperationException("hotelName already exists");
            }

            back.Hotels.Update(hotel);

            back.SaveChanges();
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }

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

    public void CreateRoom(Room newPad)
    {
        back.Rooms.Add(newPad);

        back.SaveChanges();
    }

    public Room? GetRoom(int uniqueRoomId)
    {
        return back.Rooms.Find(uniqueRoomId);
    }

    public void UpdateRoom(Room room)
    {
        var current = back.Rooms.Find(room.Urid);

        if (current == default)
        {
            throw new InvalidOperationException("roomId not found");
        }

       //reset invariant

        back.SaveChanges();
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

    public IQueryable<Room> GetHotelRooms(int hotelId)
    {
        return back.Rooms
            
            .Where(hotel => hotel.HotelId == hotelId);
    }



    public int CreateEmployee(string lastName, string firstName, long socialSecurityNumber)
    {
        var newbye = new Employee(lastName, firstName, socialSecurityNumber);

        back.Employees.Add(newbye);

        back.SaveChanges();

        return newbye.EmployeeId;
    }

    public Employee? GetEmployee(int employeeId)
    {
        return back.Employees
            
            .Find(employeeId);
    }

    public void UpdateEmployee(Employee employee)
    {
        var current = back.Employees.Find(employee.EmployeeId);

        if (current == default)
        {
            throw new InvalidOperationException("employeeId not found");
        }

        employee = employee with
        {
            SocialSecurityNumber = current.SocialSecurityNumber //prevent SSN update
        };

        back.Employees.Update(employee);

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