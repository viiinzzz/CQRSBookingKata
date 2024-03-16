
namespace CQRSBookingKata.API.Repositories;

public partial class AssetsRepository
{
    public int Create(NewEmployee spec)
    {
        var employee = new Employee(spec.LastName, spec.FirstName, spec.SocialSecurityNumber);

        back.Employees.Add(employee);
        back.SaveChanges();

        return employee.EmployeeId;
    }

    public int Create(NewHotel spec)
    {
        var hotel = new Hotel(spec.HotelName, spec.Latitude, spec.Longitude);

        back.Hotels.Add(hotel);
        back.SaveChanges();

        return hotel.HotelId;
    }

    public void Create(Room room)
    {
        back.Rooms.Add(room);

        back.SaveChanges();
    }

}