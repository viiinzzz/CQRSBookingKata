namespace BookingKata.Admin;


public class AdminQueryService
(
    IAdminRepository admin,
    IGazetteerService geo
)
{
    public IQueryable<RoomDetails> GetRoomDetails(int hotelId, int[]? exceptRoomNumbers)
    {
        var hotel = admin.GetHotel(hotelId);

        if (hotel == default)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(hotelId));
        }

        if (hotel.Disabled)
        {
            throw new ArgumentException(ReferenceDisabled, nameof(hotelId));
        }

        var hotelCell = geo.RefererGeoIndex(hotel);

        if (hotelCell == null)
        {
            throw new ArgumentException(ReferenceNotIndexed, nameof(hotelId));
        }

        //sales search will be performed against known cities list,
        //hence determining nearest known city name for geo-indexing

        var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelCell);
        var nearestKnownCityName = nearestKnownCity?.name;

        var rooms = admin.Rooms(hotelId);

        if (exceptRoomNumbers != default)
        {
            rooms = rooms
                .Where(room => !exceptRoomNumbers.Contains(room.RoomNum));
        }

        var floorNumMax = new UniqueRoomId(rooms.Max(room => room.Urid)).FloorNum;

        var roomDetails = rooms
            .Select(room => new RoomDetails(
                room.PersonMaxCount,
                hotel.Latitude,
                hotel.Longitude,
                hotel.HotelName,
                hotel.ranking,
                nearestKnownCityName,
                new UniqueRoomId(room.Urid).FloorNum,
                floorNumMax,
                room.Urid
            ));

        return roomDetails;
    }


  
}