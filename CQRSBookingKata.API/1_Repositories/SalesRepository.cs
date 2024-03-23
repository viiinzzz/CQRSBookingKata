
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

        => _sales.Vacancies
            .Where(vacancy => !vacancy.Cancelled)
            .AsNoTracking();

    public void AddVacancies(IEnumerable<Vacancy> newVacancies, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var toBeAdded = 
                
                from newVacancy in newVacancies
                //left outer join
                join curVacancy in _sales.Vacancies  
                    on newVacancy.VacancyId equals curVacancy.VacancyId into alreadyExist
                from already in alreadyExist.DefaultIfEmpty()
                where already is null
                select newVacancy;

            if (!toBeAdded.Any())
            {
                return;
            }

            _sales.Vacancies.AddRange(toBeAdded);

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

            var toBeRemoved = _sales.Vacancies
                // .AsNoTracking()
                .Where(match => vacancyIds
                    .Contains(match.VacancyId))
                .ToArray();

            if (!toBeRemoved.Any())
            {
                return;
            }

            _sales.Vacancies.RemoveRange(toBeRemoved);

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