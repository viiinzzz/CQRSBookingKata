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


public interface IAssetsRepository
{
    IQueryable<Employee> Employees { get; }
    IQueryable<Hotel> Hotels { get; }
    IQueryable<Room> Rooms(int hotelId);


    int Create(NewEmployee employee);
    Employee? GetEmployee(int employeeId);
    void Update(int employeeId, UpdateEmployee update);
    void DisableEmployee(int employeeId, bool disable);


    int Create(NewHotel hotel);
    Hotel? GetHotel(int hotelId);
    void Update(int hotelId, UpdateHotel update);
    void DisableHotel(int hotelId, bool disable);

    void Create(Room room);
    Room? GetRoom(int uniqueRoomId);
    void Update(int roomId, UpdateRoom update);
    void DeleteRoom(int roomId);


    void Create(NewRooms rooms);
    int GetFloorNextRoomNumber(int hotelId, int floorNum);

}