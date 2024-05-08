namespace BookingKata.API.Demo;

public partial class DemoService
{

    private void Fake_Customers(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");

            {
                var message = "Demo: Seeding Customers...";

                bus.Notify(new AdvertisementNotification(message, [])
                {
                    Originator = originator,
                    Immediate = true
                });
            }

            demoContext.FakeCustomerIds = FakeHelper
                .GenerateFakeCustomers(CustomerCount)
                .Select(fake =>
                {
                    var customerId = sales.CreateCustomer(fake.EmailAddress);

                    demoContext.FakeCustomers[customerId] = fake;

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