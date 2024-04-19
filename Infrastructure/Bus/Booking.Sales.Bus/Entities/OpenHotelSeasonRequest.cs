namespace BookingKata.Infrastructure.Bus.Sales;

public record OpenHotelSeasonRequest(
    string openingDateUtc,
    string closingDateUtc,

    int[]? exceptRoomNumbers,
    int hotelId
);