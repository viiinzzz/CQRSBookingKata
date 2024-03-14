namespace CQRSBookingKata.Sales;

public record StayRequest
(
    int PersonCount,
    
    double Latitude,
    double Longitude,
    int maxKm,

    DateTime ArrivalDate,
    DateTime DepartureDate,

    double PriceMin,
    double PriceMax,
    string Currency
);