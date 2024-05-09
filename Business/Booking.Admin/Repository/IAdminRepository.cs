namespace BookingKata.Admin;

//housekeeping, guest services, food and beverage service, security, IT, maintenance, HR

public interface IAdminRepository
{
    IQueryable<Employee> Employees { get; }
    int Create(CreateEmployeeRequest employee);
    Employee? GetEmployee(int employeeId);
    Employee Update(int employeeId, UpdateEmployee update);
    Employee DisableEmployee(int employeeId, bool disable);


    IQueryable<Hotel> Hotels { get; }
    int Create(NewHotel hotel);
    Hotel? GetHotel(int hotelId);
    Hotel Update(int hotelId, ModifyHotel modify);
    Hotel DisableHotel(int hotelId, bool disable);


    IQueryable<Room> Rooms(int hotelId);
    int Create(Room room);
    Room? GetRoom(int uniqueRoomId);
    Room Update(int roomId, UpdateRoom update);
    void DeleteRoom(int roomId);
    int[] Create(CreateHotelFloor newRooms);
    int[] GetFloorNextRoomNumbers(int hotelId, int floorNum, int roomCount);
}