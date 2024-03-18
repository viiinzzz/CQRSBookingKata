
namespace CQRSBookingKata.API;

public partial class AssetsRepository
{
    public Hotel? GetHotel(int hotelId) => _back.Hotels.Find(hotelId);

    public Room? GetRoom(int uniqueRoomId) => _back.Rooms.Find(uniqueRoomId);

    public Employee? GetEmployee(int employeeId) => _back.Employees.Find(employeeId);


    public int GetFloorNextRoomNumber(int hotelId, int floorNum)
    {
        var hotel = GetHotel(hotelId);

        if (hotel == default)
        {
            throw new HotelDoesNotExistException();
        }

        var floorRooms = Rooms(hotelId)

            .Where(room => room.FloorNum == floorNum);

        if (!floorRooms.Any())
        {
            return floorNum * 100 + 1;
        }

        var max = floorRooms

            .Max(room => room.RoomNum);

        if (max % 100 == 99)
        {
            throw new FloorNumbersDepletedException();
        }

        //todo skip forbidden room numbers

        return max + 1;
    }

}