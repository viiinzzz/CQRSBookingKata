namespace BookingKata.Admin;

public record CreateEmployeeRequest
(
    string LastName = default, 
    string FirstName = default, 
    long SocialSecurityNumber = default
);