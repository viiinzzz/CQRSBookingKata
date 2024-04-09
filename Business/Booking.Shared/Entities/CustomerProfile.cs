namespace BookingKata.Shared;


public record CustomerProfile(int CustomerId)
{
    public IList<Booking> BookingHistory { get; set; }
}