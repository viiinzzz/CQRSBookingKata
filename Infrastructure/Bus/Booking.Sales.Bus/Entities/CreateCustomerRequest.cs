namespace BookingKata.Infrastructure.Bus.Sales;

public record CreateCustomerRequest
(
    string EmailAddress = default
);