namespace BookingKata.Admin;

public record TimeForwardRequest
(
    int days = default,
    double? speedFactor = default
);