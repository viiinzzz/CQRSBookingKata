namespace CQRSBookingKata.API.Demo;

public partial class DemoService
{
    public int[] fakeStaffIds { get; private set; }
    public int[] fakeManagerIds { get; private set; }

    private void Fake_Employees(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            fakeStaffIds = RandomHelper
                .GenerateFakeEmployees(HotelCount * StaffPerHotel)
                .Select(fake =>
                {
                    var employeeId =
                        _admin.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                    var payrollId = _money.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

                    return employeeId;
                })
                .ToArray();

            fakeManagerIds = RandomHelper
                .GenerateFakeEmployees(HotelCount * ManagerPerHotel)
                .Select(fake =>
                {
                    var employeeId =
                        _admin.Create(new NewEmployee(fake.LastName, fake.FirstName, fake.SocialSecurityNumber));
                    var payrollId = _money.Enroll(employeeId, fake.MonthlyIncome, fake.Currency);

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