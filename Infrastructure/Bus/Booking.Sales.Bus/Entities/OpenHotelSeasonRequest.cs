namespace BookingKata.Infrastructure.Bus.Sales;

public record OpenHotelSeasonRequest(
    string openingDate,
    string closingDate,

    int[]? exceptRoomNumbers,
    int hotelId
);