using CQRSBookingKata.API.Databases;
using CQRSBookingKata.Assets;
using CQRSBookingKata.Sales;

namespace CQRSBookingKata.API.Repositories;

public class SalesRepository(BookingFrontContext front) : ISalesRepository
{

    public void AddStayProposition(StayProposition proposition)
    {
        front.Propositions.Add(proposition);

        front.SaveChanges();
    }

    public bool HasActiveProposition(int urid, DateTime arrival, DateTime departure)
    {
        return front.Propositions
            
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

        front.Customers.Add(newbie);

        front.SaveChanges();

        return newbie.CustomerId;
    }

    public Customer? GetCustomer(int customerId)
    {
        return front.Customers

            .Find(customerId);
    }

    public int? FindCustomer(string emailAddress)
    {
        return front.Customers
            
            .FirstOrDefault(customer => customer.EmailAddress == emailAddress)
            
            ?.CustomerId;
    }

    public void UpdateCustomer(Customer customer)
    {
        using var transaction = front.Database.BeginTransaction();

        try
        {
            var customer2Id = FindCustomer(customer.EmailAddress);

            if (customer2Id != customer.CustomerId)
            {
                throw new InvalidOperationException("email already exists");
            }

            front.Customers.Update(customer);

            front.SaveChanges();
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
        using var transaction = front.Database.BeginTransaction();

        try
        {
            var customer = GetCustomer(customerId);

            if (customer == default)
            {
                throw new InvalidOperationException("customerId not found");
            }

            front.Customers.Update(customer with
            {
                Disabled = disable
            });

            front.SaveChanges();
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }



    public IQueryable<Vacancy> Stock => front.Stock.AsQueryable();

    public void AddVacancy(IEnumerable<Vacancy> vacancies)
    {
        using var transaction = front.Database.BeginTransaction();

        try
        {
            var toBeAdded = front.Stock

                .Where(match => vacancies

                    .All(vacancy => vacancy.VacancyId != match.VacancyId));

            if (!toBeAdded.Any())
            {
                return;
            }

            front.Stock.AddRange(toBeAdded);

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
        using var transaction = front.Database.BeginTransaction();

        try
        {
            var toBeRemoved = front.Stock

                .Where(match => vacancyIds.Contains(match.VacancyId));

            if (!toBeRemoved.Any())
            {
                return;
            }

            front.Stock.RemoveRange(toBeRemoved);

            front.SaveChanges();

            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();

            throw new ServerErrorException(e);
        }
    }
}