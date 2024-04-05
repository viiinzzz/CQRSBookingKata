namespace BookingKata.Sales;

public record Customer(
    string EmailAddress,

    int CustomerId = default,
    bool Disabled = false
);