namespace CQRSBookingKata.Billing;

public record Booking
(
    DateTime ArrivalDate,
    DateTime DepartureDate,
    int PersonCount,

    int UniqueRoomId,
    int CustomerId,
    int BookingId = 0,
    bool Cancelled = false
);