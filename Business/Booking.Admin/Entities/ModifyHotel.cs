namespace BookingKata.Admin;

public record ModifyHotel
(
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