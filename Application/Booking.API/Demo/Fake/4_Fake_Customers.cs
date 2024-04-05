namespace BookingKata.API.Demo;

public partial class DemoService
{

    private void Fake_Customers(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            bus.Notify(new NotifyMessage(Recipient.Audit, Verb.Audit.Information)
            {
                Message = "Demo: Seeding Customers...",
                Immediate = true
            });

            demo.FakeCustomerIds = FakeHelper
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