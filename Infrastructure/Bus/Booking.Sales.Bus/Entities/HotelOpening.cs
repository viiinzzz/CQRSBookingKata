namespace BookingKata.Infrastructure.Bus.Sales;

public record HotelOpening
(
    int hotelId = default,
    string openingDate = default,
    string closingDate = default
);