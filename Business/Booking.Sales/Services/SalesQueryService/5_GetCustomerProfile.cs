namespace BookingKata.Sales;

public partial class SalesQueryService
{
    public CustomerProfile? GetCustomerProfile(int customerId)
    {
        if (!sales.Customers.Any(customer => customer.CustomerId == customerId))
        {
            return default;
        }

        return new CustomerProfile(customerId)
        {
            BookingHistory =
                (
                    from booking in sales.Bookings
                    where booking.CustomerId == customerId &&
                          !booking.Cancelled
                    select booking
                )
                .ToList()
        };
    }
}