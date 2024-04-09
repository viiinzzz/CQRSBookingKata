namespace BookingKata.Infrastructure.Bus.Admin;

public record RoomDetailsRequest(int hotelId, int[]? exceptRoomNumbers);