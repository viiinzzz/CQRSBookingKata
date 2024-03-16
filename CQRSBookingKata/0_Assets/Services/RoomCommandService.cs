
using CQRSBookingKata.Sales;

namespace CQRSBookingKata.Assets;

//housekeeping, guest services, food and beverage service, security, IT, maintenance, HR
public class RoomCommandService(IAssetsRepository assets)
{
    private int GetFloorNextRoomNumber(int hotelId, int floorNum)
    {
        var hotel = assets.GetHotel(hotelId);

        if (hotel == default)
        {
            throw new HotelDoesNotExistException();
        }

        var floorRooms = assets

            .Rooms(hotelId)

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

    public void AddRooms(int hotelId, int floorNum, int roomCount, int personMaxCount)
    {
        //transaction

        var hotel = assets.GetHotel(hotelId);

        if (hotel == default)
        {
            throw new HotelDoesNotExistException();
        }

        for (int i = 0; i < roomCount; i++)
        {
            int roomNum = GetFloorNextRoomNumber(hotelId, floorNum);

            var urid = new UniqueRoomId(hotelId, floorNum, roomNum);

            var room = new Room(urid.Value, personMaxCount);
            
            assets.Create(room);
        }
    }
}