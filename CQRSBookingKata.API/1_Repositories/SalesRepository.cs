
namespace CQRSBookingKata.API;

public class SalesRepository(IDbContextFactory factory, ITimeService DateTime) : ISalesRepository
{
    private readonly BookingFrontContext _front = factory.CreateDbContext<BookingFrontContext>();

    public void AddStayProposition(StayProposition proposition)
    {
        _front.Propositions.Add(proposition);

        _front.SaveChanges();
    }

    public bool HasActiveProposition(int urid, DateTime arrival, DateTime departure)
    {
        return _front.Propositions
            
            .Any(prop => 
                
                prop.Urid == urid &&
                
                prop.OptionStartsUtc <= DateTime.UtcNow &&
                prop.OptionEndsUtc >= DateTime.UtcNow  &&
                
                (
                    (prop.ArrivalDate >= arrival && prop.DepartureDate <= departure) ||
                    (prop.ArrivalDate <= arrival && prop.DepartureDate >= departure) ||
                    (prop.ArrivalDate <= arrival && prop.DepartureDate >= arrival) ||
                    (prop.ArrivalDate <= departure && prop.DepartureDate >= departure)
                ));
    }


    public int CreateCustomer(string emailAddress)
    {
        var customerId = FindCustomer(emailAddress);

        if (customerId != default)
        {
            throw new EmailAlreadyRegisteredException();
        }

        var newbie = new Customer(emailAddress);

        _front.Customers.Add(newbie);

        _front.SaveChanges();

        return newbie.CustomerId;
    }

    public Customer? GetCustomer(int customerId)
    {
        return _front.Customers

            .Find(customerId);
    }

    public int? FindCustomer(string emailAddress)
    {
        return _front.Customers
            
            .FirstOrDefault(customer => customer.EmailAddress == emailAddress)
            
            ?.CustomerId;
    }

    public void UpdateCustomer(Customer customer)
    {
        using var transaction = _front.Database.BeginTransaction();

        try
        {
            var customer2Id = FindCustomer(customer.EmailAddress);

            if (customer2Id != customer.CustomerId)
            {
                throw new InvalidOperationException("email already exists");
            }

            _front.Customers.Update(customer);

            _front.SaveChanges();
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }

    public void DisableCustomer(int customerId, bool disable)
    {
        using var transaction = _front.Database.BeginTransaction();

        try
        {
            var customer = GetCustomer(customerId);

            if (customer == default)
            {
                throw new InvalidOperationException("customerId not found");
            }

            _front.Customers.Update(customer with
            {
                Disabled = disable
            });

            _front.SaveChanges();
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }



    public IQueryable<Vacancy> Stock => _front.Stock.AsQueryable();

    public void AddVacancy(IEnumerable<Vacancy> vacancies)
    {
        using var transaction = _front.Database.BeginTransaction();

        try
        {
            var toBeAdded = _front.Stock

                .Where(match => vacancies

                    .All(vacancy => vacancy.VacancyId != match.VacancyId));

            if (!toBeAdded.Any())
            {
                return;
            }

            _front.Stock.AddRange(toBeAdded);

            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }

    public void RemoveVacancies(IEnumerable<long> vacancyIds)
    {
        using var transaction = _front.Database.BeginTransaction();

        try
        {
            var toBeRemoved = _front.Stock

                .Where(match => vacancyIds.Contains(match.VacancyId));

            if (!toBeRemoved.Any())
            {
                return;
            }

            _front.Stock.RemoveRange(toBeRemoved);

            _front.SaveChanges();

            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }
}