namespace CQRSBookingKata.Admin;

public record Employee(
    string LastName,
    string FirstName,
    long SocialSecurityNumber,

    int EmployeeId = 0,
    bool Disabled = false
);