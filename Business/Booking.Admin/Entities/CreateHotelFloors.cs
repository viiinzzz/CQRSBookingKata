namespace BookingKata.Admin;

public record CreateHotelFloors
(
    int HotelId = default,
    int FloorCount = default,
    int RoomPerFloor = default,
    int PersonPerRoom = default
);