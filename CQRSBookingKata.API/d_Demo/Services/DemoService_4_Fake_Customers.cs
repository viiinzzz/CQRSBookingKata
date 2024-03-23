namespace CQRSBookingKata.API.Demo;

public partial class DemoService
{

    private void Fake_Customers(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            demo.FakeCustomerIds = RandomHelper
                .GenerateFakeCustomers(CustomerCount)
                .Select(fake =>
                {
                    var customerId = sales.CreateCustomer(fake.EmailAddress, scoped: false);

                    demo.FakeCustomers[customerId] = fake;

                    return customerId;
                })
                .ToArray();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Employees)}' failed", e);
        }
    }
}