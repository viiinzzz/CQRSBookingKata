namespace CQRSBookingKata.API;

public class MoneyRepository(IDbContextFactory factory, ITimeService DateTime) : IMoneyRepository, ITransactionable
{
    private readonly BookingMoneyContext _money = factory.CreateDbContext<BookingMoneyContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _money;


    public void AddInvoice(Invoice invoice, bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            _money.Invoices.Add(invoice);
            _money.SaveChanges();
            _money.Entry(invoice).State = EntityState.Detached;

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

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