namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public Id EnrollEmployee
    (
        int employeeId,
        double monthlyIncome,
        string currency,

        long correlationId1,
        long correlationId2
    )
    {
        var payrollId = money.EnrollEmployee(employeeId, monthlyIncome, currency);

        return new Id(payrollId);
    }
}