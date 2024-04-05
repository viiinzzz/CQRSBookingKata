namespace BookingKata.Admin;

public record RoomDetails
(
    int PersonMaxCount,

    double Latitude,
    double Longitude,

    string HotelName,
    int HotelRank,
    string? NearestKnownCityName,

    int FloorNum,
    int FloorNumMax,

    int Urid
);
