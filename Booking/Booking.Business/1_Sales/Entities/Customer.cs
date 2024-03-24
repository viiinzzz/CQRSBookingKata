namespace CQRSBookingKata.Sales;

public record Customer(
    string EmailAddress,

    int CustomerId = 0,
    bool Disabled = false
);