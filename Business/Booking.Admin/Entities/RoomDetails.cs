namespace BookingKata.Admin;

public record RoomDetails(
    int PersonMaxCount,

    double Latitude,
    double Longitude,

    string HotelName,
    string NearestKnownCityName,

    int Urid
);