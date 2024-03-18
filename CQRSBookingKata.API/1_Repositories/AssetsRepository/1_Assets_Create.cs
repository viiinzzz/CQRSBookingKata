
namespace CQRSBookingKata.API;

public partial class AssetsRepository
{
    public int Create(NewEmployee spec)
    {
        var employee = new Employee(spec.LastName, spec.FirstName, spec.SocialSecurityNumber);

        _back.Employees.Add(employee);
        _back.SaveChanges();

        return employee.EmployeeId;
    }

    public int Create(NewHotel spec)
    {
        var hotel = new Hotel(spec.HotelName, spec.Latitude, spec.Longitude);

        _back.Hotels.Add(hotel);
        _back.SaveChanges();

        return hotel.HotelId;
    }

    public void Create(Room room)
    {
        _back.Rooms.Add(room);

        _back.SaveChanges();
    }


    public void Create(NewRooms rooms)
    {
        //transaction

        var hotel = GetHotel(rooms.HotelId);

        if (hotel == default)
        {
            throw new HotelDoesNotExistException();
        }

        for (int i = 0; i < rooms.RoomCount; i++)
        {
            int roomNum = GetFloorNextRoomNumber(rooms.HotelId, rooms.FloorNum);

            var urid = new UniqueRoomId(rooms.HotelId, rooms.FloorNum, roomNum);

            var room = new Room(urid.Value, rooms.PersonMaxCount);

            Create(room);
        }
    }

}