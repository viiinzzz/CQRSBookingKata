namespace BookingKata.ThirdParty;

public interface IPricingQueryService
{
    Price GetPrice
    (
        //room
        int personMaxCount, int floorNum, int floorNumMax, int hotelRank, double latitude, double longitude,

        //booking
        int personCount, DateTime arrivalDate, DateTime departureDate, string? currency, CustomerProfile? customerProfile
    );
}
