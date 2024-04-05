namespace BookingKata.Admin;

public record UpdateEmployee(
    string? LastName = default, 
    string? FirstName = default,
    bool? Disabled = default
);