
namespace CQRSBookingKata.API;

public class SalesRepository(IDbContextFactory factory, ITimeService DateTime) : ISalesRepository, ITransactionable
{
    private readonly BookingSalesContext _sales = factory.CreateDbContext<BookingSalesContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _sales;


    public void AddStayProposition(StayProposition proposition)
    {
        var entity = _sales.Propositions.Add(proposition);
        _sales.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public bool HasActiveProposition(int urid, DateTime arrival, DateTime departure)
    {
        return _sales.Propositions
            .AsNoTracking()
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


    public int CreateCustomer(string emailAddress, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var customerId = FindCustomer(emailAddress);

            if (customerId != default)
            {
                throw new EmailAlreadyRegisteredException();
            }

            var newbie = new Customer(emailAddress);

            var entity = _sales.Customers.Add(newbie);
            _sales.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();

            return newbie.CustomerId;
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public Customer? GetCustomer(int customerId)
    {
        var customer = _sales.Customers
            .Find(customerId);

        if (customer == default)
        {
            return default;
        }

        _sales.Entry(customer).State = EntityState.Detached;

        return customer;
    }

    public int? FindCustomer(string emailAddress)
    {
        var customer = _sales.Customers
            .FirstOrDefault(customer => customer.EmailAddress == emailAddress);

        if (customer == default)
        {
            return default;
        }

        _sales.Entry(customer).State = EntityState.Detached;

        return customer.CustomerId;
    }

    public void UpdateCustomer(Customer customer, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var customer2Id = FindCustomer(customer.EmailAddress);

            if (customer2Id != customer.CustomerId)
            {
                throw new InvalidOperationException("email already exists");
            }

            var entity = _sales.Customers.Update(customer);
            _sales.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public void DisableCustomer(int customerId, bool disable, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var customer = GetCustomer(customerId);

            if (customer == default)
            {
                throw new InvalidOperationException("customerId not found");
            }

            var entity = _sales.Customers.Update(customer with
            {
                Disabled = disable
            });

            _sales.SaveChanges();
            entity.State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }



    public IQueryable<Vacancy> Vacancies 

        => _sales.Stock
            .AsNoTracking();

    public void AddVacancies(IEnumerable<Vacancy> vacancies, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var toBeAdded = _sales.Stock



                //TODO
                //investigate: Cannot use multiple context instances within a single query execution. Ensure the query uses a single context instance
                //temp fix!!! means load all data into memory.... not really wished
                .ToList()



                // .AsNoTracking()
                .Where(match => vacancies
                    .All(vacancy => vacancy.VacancyId != match.VacancyId))
                .ToArray();

            if (!toBeAdded.Any())
            {
                return;
            }

            _sales.Stock.AddRange(toBeAdded);

            foreach (var vacancy in toBeAdded)
            {
                _sales.Entry(vacancy).State = EntityState.Detached;
            }

            _sales.SaveChanges();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public void RemoveVacancies(IEnumerable<long> vacancyIds, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var toBeRemoved = _sales.Stock
                // .AsNoTracking()
                .Where(match => vacancyIds
                    .Contains(match.VacancyId))
                .ToArray();

            if (!toBeRemoved.Any())
            {
                return;
            }

            _sales.Stock.RemoveRange(toBeRemoved);

            foreach (var vacancy in toBeRemoved)
            {
                _sales.Entry(vacancy).State = EntityState.Detached;
            }

            _sales.SaveChanges();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }
}