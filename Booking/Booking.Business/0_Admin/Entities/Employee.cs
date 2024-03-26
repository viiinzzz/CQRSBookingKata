namespace BookingKata.Admin;

public record Employee(
    string LastName,
    string FirstName,
    long SocialSecurityNumber,

    int EmployeeId = default,
    bool Disabled = false
);