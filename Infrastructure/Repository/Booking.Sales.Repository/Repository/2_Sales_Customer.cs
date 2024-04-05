using System.Runtime.ExceptionServices;

namespace BookingKata.Infrastructure.Storage;

public partial class SalesRepository
{
    public IQueryable<Customer> Customers

        => _sales.Customers
            .AsNoTracking();

    public int CreateCustomer(string emailAddress)
    {
        var customerId = FindCustomer(emailAddress);

        if (customerId != default)
        {
            throw new ArgumentException(Business.Common.Exceptions.ReferenceInvalid, nameof(emailAddress));
        }

        var newbie = new Customer(emailAddress);

        var entity = _sales.Customers.Add(newbie);
        _sales.SaveChanges();
        entity.State = EntityState.Detached;

        return newbie.CustomerId;
    }

    public Customer? GetCustomer(int customerId)
    {
        var customer = _sales.Customers.Find(customerId);

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

    public void UpdateCustomer(Customer customer)
    {
        var customer2Id = FindCustomer(customer.EmailAddress);

        if (customer2Id != customer.CustomerId)
        {
            throw new ArgumentException(Business.Common.Exceptions.ReferenceMismatch, nameof(customer.CustomerId));
        }

        var entity = _sales.Customers.Update(customer);

        _sales.SaveChanges();
        entity.State = EntityState.Detached;
    }

    public void DisableCustomer(int customerId, bool disable)
    {
        var customer = GetCustomer(customerId);

        if (customer == default)
        {
            throw new ArgumentException(Business.Common.Exceptions.ReferenceInvalid, nameof(customerId));
        }

        var entity = _sales.Customers.Update(customer with
        {
            Disabled = disable
        });

        _sales.SaveChanges();
        entity.State = EntityState.Detached;
    }
}