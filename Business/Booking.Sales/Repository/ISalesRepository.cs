namespace BookingKata.Sales;

public interface ISalesRepository
{
    IQueryable<Customer> Customers { get; }
    int CreateCustomer(string emailAddress);
    Customer? GetCustomer(int customerId);
    int? FindCustomer(string emailAddress);
    void UpdateCustomer(Customer customer);
    void DisableCustomer(int customerId, bool disable);


    IQueryable<Vacancy> Vacancies { get; }
    void AddVacancies(IEnumerable<Vacancy> newVacancies);
    void RemoveVacancies(IEnumerable<long> vacancyIds);


    IQueryable<StayProposition> Propositions { get; }
    void AddStayProposition(StayProposition proposition);
    bool HasActiveProposition(DateTime now, int urid, DateTime arrival, DateTime departure);


    IQueryable<Shared.Booking> Bookings { get; }
    int AddBooking(Shared.Booking booking);

    Shared.Booking CancelBooking(int bookingId);
}