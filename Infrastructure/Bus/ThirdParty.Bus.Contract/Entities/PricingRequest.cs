namespace Common.Infrastructure.Network;

public record PricingRequest
(
    //room
    int personMaxCount,
    int floorNum,
    int floorNumMax,
    int hotelRank,
    int latitude,
    int longitude,

    //booking
    int personCount,
    string arrivalDateUtc,
    string departureDateUtc,
    string? currency,
    string? customerProfileJson
);