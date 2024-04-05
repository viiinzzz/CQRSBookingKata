namespace BookingKata.Sales;

public record Customer(
    string EmailAddress,

    int CustomerId = default,
    bool Disabled = false
);


public record CustomerProfile(int CustomerId)
{
    public IList<Booking> Booked { get; set; }
}