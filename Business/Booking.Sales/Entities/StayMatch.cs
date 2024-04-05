namespace BookingKata.Sales;

public record StayMatch
(
    int PersonCount,
    DateTime ArrivalDate,
    DateTime DepartureDate,

    double Price,
    string Currency,

    int Urid
);