namespace BookingKata.Shared;

public record RoomDetails
(
    int PersonMaxCount,

    double Latitude,
    double Longitude,

    string HotelName,
    int HotelRank,
    string? NearestKnownCityName,

    double EarliestCheckInHours,
    double LatestCheckOutHours,

    int FloorNum,
    int FloorNumMax,

    int Urid
);
