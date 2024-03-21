namespace CQRSBookingKata.Sales;

public interface ISalesRepository
{
    int CreateCustomer(string emailAddress, bool scoped);
    Customer? GetCustomer(int customerId);
    int? FindCustomer(string emailAddress);
    void UpdateCustomer(Customer customer, bool scoped);
    void DisableCustomer(int customerId, bool disable, bool scoped);


    IQueryable<Vacancy> Stock { get; }
    void AddVacancies(IEnumerable<Vacancy> vacancies, bool scoped);
    void RemoveVacancies(IEnumerable<long> vacancyIds, bool scoped);
    void AddStayProposition(StayProposition proposition);
    bool HasActiveProposition(int urid, DateTime arrival, DateTime departure);
}