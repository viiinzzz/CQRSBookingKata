namespace BookingKata.API.Demo;

public partial class DemoService
{

    private void Fake_Customers()
    {
        try
        {
            {
                var message = "Demo: Seeding Customers...";

                bus.Notify(new AdvertisementNotification(message, [])
                {
                    Originator = originator,
                    Immediate = true
                });
            }

            int createCustomer(FakeHelper.FakeCustomer fake)
            {
                var createCustomerRequest = new CreateCustomerRequest
                {
                    EmailAddress = fake.EmailAddress
                };

                var customerId = bus.AskResult<Id>(Recipient.Sales, Verb.Sales.RequestCreateCustomer,
                    createCustomerRequest,
                    originator);

                if (customerId == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(customerId));
                }

                demoContext.FakeCustomers[customerId.id] = fake;

                return customerId.id;
            }

            demoContext.FakeCustomerIds = FakeHelper
                .GenerateFakeCustomers(CustomerCount)
                .Select(createCustomer)
                .ToArray();

        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Employees)}' failed", e);
        }
    }
}