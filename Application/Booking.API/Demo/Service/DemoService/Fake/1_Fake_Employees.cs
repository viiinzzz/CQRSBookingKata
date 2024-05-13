namespace BookingKata.API.Demo;

public partial class DemoService
{

    private void Fake_Employees()
    {
        try
        {
            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");
            

            int createAndEnrollEmployee(FakeHelper.FakeEmployee fakeEmployee)
            {
                var employeeId = bus.AskResult<Id>(Recipient.Admin,Verb.Admin.RequestCreateEmployee,
                    new CreateEmployeeRequest
                    {
                        LastName = fakeEmployee.LastName,
                        FirstName = fakeEmployee.FirstName,
                        SocialSecurityNumber = fakeEmployee.SocialSecurityNumber
                    }, 
                    originator);

                if (employeeId == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(employeeId));
                }
                    
                var payrollId = bus.AskResult<Id>(Support.Services.Billing.Recipient, Support.Services.Billing.Verb.RequestPayroll,
                    new PayrollRequest
                    {
                        employeeId = employeeId.id,
                        monthlyIncome = fakeEmployee.MonthlyIncome,
                        currency = fakeEmployee.Currency
                    }, 
                    originator);

                if (payrollId == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(payrollId));
                }

                return employeeId.id;
            }


            {
                var message = "Demo: Seeding Staff...";

                bus.Notify(new AdvertisementNotification(message, [])
                {
                    Originator = originator,
                    Immediate = true
                });

                demoContext.FakeStaffIds = FakeHelper
                    .GenerateFakeEmployees(HotelCount * StaffPerHotel)
                    .AsParallel()
                    .Select(createAndEnrollEmployee)
                    .ToArray();
            }


            {
                var message = "Demo: Seeding Managers...";

                bus.Notify(new AdvertisementNotification(message, [])
                {
                    Originator = originator,
                    Immediate = true
                });

                demoContext.FakeManagerIds = FakeHelper
                    .GenerateFakeEmployees(HotelCount * ManagerPerHotel)
                    .Select(createAndEnrollEmployee)
                    .ToArray();
            }
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Employees)}' failed", e);
        }
    }
}