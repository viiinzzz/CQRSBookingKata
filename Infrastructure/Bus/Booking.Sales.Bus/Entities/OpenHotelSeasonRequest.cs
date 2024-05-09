namespace BookingKata.Infrastructure.Bus.Sales;

public record OpenHotelSeasonRequest
(
    string openingDateUtc = default,
    string closingDateUtc = default,

    int[]? exceptRoomNumbers = default,
    int hotelId = default
);