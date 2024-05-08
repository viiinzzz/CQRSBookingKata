namespace BookingKata.Infrastructure.Storage;

public partial class AdminRepository
{
    public int Create(CreateEmployeeRequest spec)
    {
        var employee = new Employee(spec.LastName, spec.FirstName, spec.SocialSecurityNumber);

        var entity = _admin.Employees.Add(employee);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return employee.EmployeeId;
    }

    public int Create(NewHotel spec)
    {
        var hotel = new Hotel(spec.HotelName, spec.Latitude, spec.Longitude);

        var entity = _admin.Hotels.Add(hotel);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        geo.AddReferer(hotel, bconf.PrecisionMaxKm, default);

        return hotel.HotelId;
    }

    public int Create(Room room)
    {
        var entity = _admin.Rooms.Add(room);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return room.Urid;
    }


    public int[] Create(CreateHotelFloor newRooms)
    {
        var hotel = GetHotel(newRooms.HotelId);

        if (hotel == default)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(newRooms.HotelId));
        }

      
        var roomNumbers = GetFloorNextRoomNumbers(newRooms.HotelId, newRooms.FloorNum, newRooms.RoomCount);

        var urids = roomNumbers
            .Select(roomNum =>
            {
                var urid = new UniqueRoomId(newRooms.HotelId, newRooms.FloorNum, roomNum);

                var room = new Room(urid.Value, urid.HotelId, urid.RoomNum, urid.FloorNum, newRooms.PersonMaxCount);

                Create(room);

                return urid.Value;
            })
            .ToArray();

        return urids;
      
    }


}