namespace BookingKata.Shared;

public record RoomDetailsRequest
(
    int hotelId,
    int[]? exceptRoomNumbers
);
