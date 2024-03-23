namespace CQRSBookingKata.Sales;

public interface ISalesRepository
{
    int CreateCustomer(string emailAddress, bool scoped);
    Customer? GetCustomer(int customerId);
    int? FindCustomer(string emailAddress);
    void UpdateCustomer(Customer customer, bool scoped);
    void DisableCustomer(int customerId, bool disable, bool scoped);


    IQueryable<Vacancy> Vacancies { get; }
    void AddVacancies(IEnumerable<Vacancy> newVacancies, bool scoped);
    void RemoveVacancies(IEnumerable<long> vacancyIds, bool scoped);
    void AddStayProposition(StayProposition proposition);
    bool HasActiveProposition(int urid, DateTime arrival, DateTime departure);
}