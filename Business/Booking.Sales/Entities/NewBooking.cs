namespace Booking.Sales.Entities;

public record NewBooking(
    int BookingId,
    DateTime ArrivalDate,
    DateTime DepartureDate,
    string LastName,
    string FirstName,
    int UniqueRoomId
);