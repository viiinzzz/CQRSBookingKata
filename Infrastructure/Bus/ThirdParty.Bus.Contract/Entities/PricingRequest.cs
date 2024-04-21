namespace Support.Infrastructure.Network;

public record PricingRequest
(
    //room
    int personMaxCount = default,
    int floorNum = default,
    int floorNumMax = default,
    int hotelRank = default,
    double latitude = default,
    double longitude = default,

    //booking
    int personCount = default,
    string arrivalDateUtc = default,
    string departureDateUtc = default,
    string? currency = default,
    string? customerProfileJson = default
);