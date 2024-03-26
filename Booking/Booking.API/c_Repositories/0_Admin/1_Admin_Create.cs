namespace BookingKata.API;

public partial class AdminRepository
{
    public int Create(NewEmployee spec)
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

        geo.AddReferer(hotel,  0.100, default, scoped: false);

        return hotel.HotelId;
    }

    public int Create(Room room)
    {
        var entity = _admin.Rooms.Add(room);
        _admin.SaveChanges();
        entity.State = EntityState.Detached;

        return room.Urid;
    }


    public int[] Create(NewRooms rooms, bool scoped)
    {
        var hotel = GetHotel(rooms.HotelId);

        if (hotel == default)
        {
            throw new HotelDoesNotExistException();
        }

        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var roomNumbers = GetFloorNextRoomNumbers(rooms.HotelId, rooms.FloorNum, rooms.RoomCount, scoped: false);

            var urids = roomNumbers
                .Select(roomNum =>
                {
                    var urid = new UniqueRoomId(rooms.HotelId, rooms.FloorNum, roomNum);

                    var room = new Room(urid.Value, urid.HotelId, urid.RoomNum, urid.FloorNum, rooms.PersonMaxCount);

                    Create(room);

                    return urid.Value;
                })
                .ToArray();

            scope?.Complete();

            return urids;
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }



    public void AddBooking(Booking booking, bool scoped)
    {
        try
        {
            using var scope = new TransactionScope();

            _admin.Bookings.Add(booking);
            _admin.SaveChanges();
            _admin.Entry(booking).State = EntityState.Detached;

            scope.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

}