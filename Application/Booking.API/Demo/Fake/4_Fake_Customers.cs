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

            bus.Notify(originator, new Notification(Recipient.Audit, InformationMessage)
            {
                Message = "Demo: Seeding Customers...",
                Immediate = true
            });

            demo.FakeCustomerIds = FakeHelper
                .GenerateFakeCustomers(CustomerCount)
                .Select(fake =>
                {
                    var customerId = sales.CreateCustomer(fake.EmailAddress);

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