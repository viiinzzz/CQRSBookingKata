namespace CQRSBookingKata.Assets;

public interface IAssetsRepository
{
    int CreateEmployee(string lastName, string firstName, long socialSecurityNumber);
    Employee? GetEmployee(int employeeId);
    void UpdateEmployee(Employee employee);
    void DisableEmployee(int employeeId, bool disable);


    int CreateHotel(string hotelName, double latitude, double longitude);
    Hotel? GetHotel(int hotelId);
    void UpdateHotel(Hotel hotel);
    void DisableHotel(int hotelId, bool disable);

    void CreateRoom(Room room);
    Room? GetRoom(int uniqueRoomId);
    void UpdateRoom(Room room);
    void DeleteRoom(int roomId);

    IQueryable<Room> GetHotelRooms(int hotelId);
}