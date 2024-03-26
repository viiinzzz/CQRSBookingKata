namespace BookingKata.API;

public partial class MoneyRepository
{

    public IQueryable<Payroll> Payrolls

        => _money.Payrolls
            .AsNoTracking();


    public int Enroll(int employeeId, double monthlyBaseIncome, string currency)
    {
        var payroll = new Payroll(monthlyBaseIncome, currency, employeeId);

        _money.Payrolls.Add(payroll);
        _money.SaveChanges();
        _money.Entry(payroll).State = EntityState.Detached;

        return payroll.PayrollId;
    }

    public void Deroll(int employeeId)
    {
        var rows = _money.Payrolls
            .Where(payroll => payroll.EmployeeId == employeeId)
            .ExecuteDelete();

        if (rows == 0)
        {
            throw new InvalidOperationException("employeeId has no payroll");
        }
    }
}