namespace Support.Infrastructure.Network;

public record PayrollRequest
(
    int employeeId = default,
    double monthlyIncome = default,
    string currency = default
);