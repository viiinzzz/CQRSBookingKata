namespace BookingKata.API;

public partial class SalesRepository
{
    public IQueryable<Customer> Customers

        => _sales.Customers
            .AsNoTracking();

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
        var customer = _sales.Customers.Find(customerId);

        if (customer == default) return default;

        _sales.Entry(customer).State = EntityState.Detached;

        return customer;
    }

    public int? FindCustomer(string emailAddress)
    {
        var customer = _sales.Customers
            .FirstOrDefault(customer => customer.EmailAddress == emailAddress);

        if (customer == default) return default;

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
}