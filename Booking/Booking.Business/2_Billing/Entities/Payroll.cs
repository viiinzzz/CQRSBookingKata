namespace BookingKata.Billing;

public record Payroll(
    double MonthlyBaseIncome,
    string Currency,

    int EmployeeId,
    int PayrollId = default
);