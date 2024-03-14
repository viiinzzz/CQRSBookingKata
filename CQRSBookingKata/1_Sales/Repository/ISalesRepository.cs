namespace CQRSBookingKata.Sales;

public interface ISalesRepository
{
    int CreateCustomer(string emailAddress);

    Customer? GetCustomer(int customerId);

    int? FindCustomer(string emailAddress);

    void UpdateCustomer(Customer customer);

    void DisableCustomer(int customerId, bool disable);

    IQueryable<Vacancy> Stock { get; }

    void AddVacancy(IEnumerable<Vacancy> vacancies);

    void RemoveVacancies(IEnumerable<long> vacancyIds);

    void AddStayProposition(StayProposition proposition);

    bool HasActiveProposition(int urid, DateTime arrival, DateTime departure);
}