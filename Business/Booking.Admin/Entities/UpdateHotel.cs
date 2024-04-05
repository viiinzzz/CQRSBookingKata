namespace BookingKata.Admin;

public record UpdateHotel(
    string? HotelName = default, 
    int? EarliestCheckInTime = default, 
    int? LatestCheckOutTime = default,
    string? LocationAddress = default,
    string? ReceptionPhoneNumber = default,
    string? Url = default,
    int? Ranking = default,
    int? ManagerId = default,
    bool? Disabled = default
);