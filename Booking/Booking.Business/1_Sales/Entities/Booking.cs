namespace CQRSBookingKata.Billing;

public record Booking
(
    DateTime ArrivalDate,
    DateTime DepartureDate,
    string LastName,
    string FirstName,
    int PersonCount,

    int UniqueRoomId,
    int CustomerId,
    int BookingId = 0,
    bool Cancelled = false
);