namespace BookingKata.Admin;


public class AdminQueryService
(
    IAdminRepository admin,
    IGazetteerService geo
)
{
    public IQueryable<RoomDetails> GetHotelRoomDetails(int hotelId, int[]? exceptRoomNumbers, int[]? onlyRoomNumbers)
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

        var floorNumMax = new UniqueRoomId(rooms.Max(room => room.Urid)).FloorNum;

        if (exceptRoomNumbers != default)
        {
            rooms = rooms
                .Where(room => !exceptRoomNumbers.Contains(room.RoomNum));
        }

        if (onlyRoomNumbers != default)
        {
            rooms = rooms
                .Where(room => onlyRoomNumbers.Contains(room.RoomNum));
        }

        var roomDetails = rooms
            .Select(room => new RoomDetails(
                room.PersonMaxCount,
                hotel.Latitude,
                hotel.Longitude,
                hotel.HotelName,
                hotel.ranking,
                nearestKnownCityName,
                hotel.EarliestCheckInHours,
                hotel.LatestCheckOutHours,
                new UniqueRoomId(room.Urid).FloorNum,
                floorNumMax,
                room.Urid
            ));

        return roomDetails;
    }




    public RoomDetails GetSingleRoomDetails(int urid)
    {
        var uniqueRoomId = new UniqueRoomId(urid);

        var hotelId = uniqueRoomId.HotelId;

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

        var floorNumMax = new UniqueRoomId(rooms.Max(room => room.Urid)).FloorNum;

        var room = rooms.FirstOrDefault(room => room.RoomNum == uniqueRoomId.RoomNum);

        if (room == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(urid));
        }

        var roomDetails = new RoomDetails(
                room.PersonMaxCount,
                hotel.Latitude,
                hotel.Longitude,
                hotel.HotelName,
                hotel.ranking,
                nearestKnownCityName,
                hotel.EarliestCheckInHours,
                hotel.LatestCheckOutHours,
                uniqueRoomId.FloorNum,
                floorNumMax,
                room.Urid
            );

        return roomDetails;
    }



    public RoomDetails[] GetManyRoomDetails(int[] urids)
    {
        var roomDetails = urids
            .Select(x => new
            {
                urid = x,
                hotelId = new UniqueRoomId(x).HotelId
            })
            .GroupBy(x => x.hotelId)
            .SelectMany( x =>
            {
                var hotelId = x.Key;
                var only = x.Select(x => x.urid).ToArray();

                return GetHotelRoomDetails(hotelId, null, only);
            })
            .ToArray();
       
        return roomDetails;
    }

}