namespace BookingKata.Admin;

public class AdminQueryService
(
    IAdminRepository admin,
    IGazetteerService geo
)
{
    public IQueryable<RoomDetails> GetHotelDetails(int hotelId, int[]? exceptRoomNumbers)
    {
        var hotel = admin.GetHotel(hotelId);

        if (hotel == default)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(hotelId));
        }

        if (hotel.Disabled)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceDisabled, nameof(hotelId));
        }

        var hotelCell = geo.RefererGeoIndex(hotel);

        if (hotelCell == null)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceNotIndexed, nameof(hotelId));
        }

        //sales search will be performed against known cities list,
        //hence determining nearest known city name for geo-indexing

        var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelCell);


        var rooms = admin.Rooms(hotelId);

        if (exceptRoomNumbers != default)
        {
            rooms = rooms
                .Where(room => !exceptRoomNumbers.Contains(room.RoomNum));
        }


        var roomDetails = rooms
            .Select(room => new RoomDetails(
                room.PersonMaxCount,
                hotel.Latitude,
                hotel.Longitude,
                hotel.HotelName,
                nearestKnownCity.name,
                room.Urid
            ));

        return roomDetails;
    }


  
}