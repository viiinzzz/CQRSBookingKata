namespace BookingKata.API.Demo;

public partial class DemoService
{

    private void Fake_Employees(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            bus.Notify(new NotifyMessage(Recipient.Audit, Verb.InformationMessage)
            {
                Message = "Demo: Seeding Staff...",
                Immediate = true
            });

            demo.FakeStaffIds = FakeHelper
                .GenerateFakeEmployees(HotelCount * StaffPerHotel)
                .Select(fake =>
                {
                    var employeeId =
                        admin.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                    var payrollId = money.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                    return employeeId;
                })
                .ToArray();

            bus.Notify(new NotifyMessage(Recipient.Audit, Verb.Audit.Information)
            {
                Message = "Demo: Seeding Managers...",
                Immediate = true
            });

            demo.FakeManagerIds = FakeHelper
                .GenerateFakeEmployees(HotelCount * ManagerPerHotel)
                .Select(fake =>
                {
                    var employeeId =
                        admin.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                    var payrollId = money.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                    return employeeId;
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