namespace BookingKata.Sales;

public record Booking
(
    DateTime ArrivalDate,
    DateTime DepartureDate,
    string LastName,
    string FirstName,
    int PersonCount,

    int UniqueRoomId,
    int CustomerId,
    int BookingId = default,
    bool Cancelled = false
);