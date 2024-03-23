namespace CQRSBookingKata.API.Demo;

public partial class DemoService
{
    public int[] fakeCustomerIds { get; private set; }
    public Dictionary<int, RandomHelper.FakeCustomer> fakeCustomers = new();

    private void Fake_Customers(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            fakeCustomerIds = RandomHelper
                .GenerateFakeCustomers(CustomerCount)
                .Select(fake =>
                {
                    var customerId = _sales.CreateCustomer(fake.EmailAddress, scoped: false);

                    fakeCustomers[customerId] = fake;

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