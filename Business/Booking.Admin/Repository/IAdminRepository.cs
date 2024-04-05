namespace BookingKata.Admin;

//housekeeping, guest services, food and beverage service, security, IT, maintenance, HR

public interface IAdminRepository
{
    IQueryable<Employee> Employees { get; }
    int Create(NewEmployee employee);
    Employee? GetEmployee(int employeeId);
    void Update(int employeeId, UpdateEmployee update);
    void DisableEmployee(int employeeId, bool disable);


    IQueryable<Hotel> Hotels { get; }
    int Create(NewHotel hotel);
    Hotel? GetHotel(int hotelId);
    void Update(int hotelId, UpdateHotel update);
    void DisableHotel(int hotelId, bool disable);


    IQueryable<Room> Rooms(int hotelId);
    int Create(Room room);
    Room? GetRoom(int uniqueRoomId);
    void Update(int roomId, UpdateRoom update);
    void DeleteRoom(int roomId);
    int[] Create(NewRooms newRooms);
    int[] GetFloorNextRoomNumbers(int hotelId, int floorNum, int roomCount);
}