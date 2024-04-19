namespace BookingKata.Shared;

public record RoomDetailsRequest
(
    int? hotelId = default,
    int[]? exceptRoomNumbers = default,
    int[]? onlyRoomNumbers = default
);
