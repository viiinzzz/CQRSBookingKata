namespace BookingKata.API.Demo;

public partial class DemoService
{

    private void Fake_Employees(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            demo.FakeStaffIds = RandomHelper
                .GenerateFakeEmployees(HotelCount * StaffPerHotel)
                .Select(fake =>
                {
                    var employeeId =
                        admin.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                    var payrollId = money.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                    return employeeId;
                })
                .ToArray();

            demo.FakeManagerIds = RandomHelper
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