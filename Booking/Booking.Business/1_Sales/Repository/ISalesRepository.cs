namespace BookingKata.Sales;

public interface ISalesRepository
{
    IQueryable<Customer> Customers { get; }
    int CreateCustomer(string emailAddress, bool scoped);
    Customer? GetCustomer(int customerId);
    int? FindCustomer(string emailAddress);
    void UpdateCustomer(Customer customer, bool scoped);
    void DisableCustomer(int customerId, bool disable, bool scoped);

    IQueryable<Vacancy> Vacancies { get; }
    void AddVacancies(IEnumerable<Vacancy> newVacancies, bool scoped);
    void RemoveVacancies(IEnumerable<long> vacancyIds, bool scoped);

    IQueryable<StayProposition> Propositions { get; }
    void AddStayProposition(StayProposition proposition);
    bool HasActiveProposition(DateTime now, int urid, DateTime arrival, DateTime departure);
}