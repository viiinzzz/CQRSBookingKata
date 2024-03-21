namespace CQRSBookingKata.Assets;

//housekeeping, guest services, food and beverage service, security, IT, maintenance, HR

public record NewEmployee(string LastName, string FirstName, long SocialSecurityNumber);

public record UpdateEmployee(
    string? LastName = default, 
    string? FirstName = default,
    bool? Disabled = default
);


public record NewHotel(string HotelName, double Latitude, double Longitude);

public record UpdateHotel(
    string? HotelName = default, 
    int? EarliestCheckInTime = default, 
    int? LatestCheckOutTime = default,
    string? LocationAddress = default,
    string? ReceptionPhoneNumber = default,
    string? Url = default,
    int? Ranking = default,
    int? ManagerId = default,
    bool? Disabled = default
);

public record NewRooms(int HotelId, int FloorNum, int RoomCount, int PersonMaxCount);

public record UpdateRoom(
    int? PersonMaxCount = default
);


public interface IAdminRepository
{
    IQueryable<Employee> Employees { get; }
    int Create(NewEmployee employee);
    Employee? GetEmployee(int employeeId);
    void Update(int employeeId, UpdateEmployee update, bool scoped);
    void DisableEmployee(int employeeId, bool disable, bool scoped);


    IQueryable<Hotel> Hotels { get; }
    int Create(NewHotel hotel);
    Hotel? GetHotel(int hotelId);
    void Update(int hotelId, UpdateHotel update, bool scoped);
    void DisableHotel(int hotelId, bool disable, bool scoped);


    IQueryable<Room> Rooms(int hotelId);
    int Create(Room room);
    Room? GetRoom(int uniqueRoomId);
    void Update(int roomId, UpdateRoom update, bool scoped);
    void DeleteRoom(int roomId, bool scoped);
    int[] Create(NewRooms rooms, bool scoped);
    int[] GetFloorNextRoomNumbers(int hotelId, int floorNum, int roomCount, bool scoped);



    IQueryable<Booking> Bookings { get; }
    void AddBooking(Booking booking, bool scoped);
}