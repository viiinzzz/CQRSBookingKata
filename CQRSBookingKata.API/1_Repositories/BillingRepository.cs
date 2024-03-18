
namespace CQRSBookingKata.API;

public class BillingRepository(IDbContextFactory factory, ITimeService DateTime) : IBillingRepository
{
    private readonly BookingBackContext _back = factory.CreateDbContext<BookingBackContext>();
    private readonly BookingSensitiveContext _sensitive = factory.CreateDbContext<BookingSensitiveContext>();

    public IQueryable<Booking> AllBookings => _back.Bookings.AsQueryable();

    public void AddBookingAndInvoice(Booking booking, Invoice invoice)
    {

        using var transaction1 = _sensitive.Database.BeginTransaction();

        try
        {
            _back.Bookings.Add(booking);
            _back.SaveChanges();


            _sensitive.Invoices.Add(invoice);
            _sensitive.SaveChanges();
            
            transaction1.Commit();
        }
        catch (Exception e)
        {
            transaction1.Rollback();

            throw new ServerErrorException(e);
        }

    }

    public int Enroll(int employeeId, double monthlyBaseIncome, string currency)
    {
        var payroll = new Payroll(monthlyBaseIncome, currency, employeeId);

        _sensitive.Payrolls.Add(payroll);

        _sensitive.SaveChanges();

        return payroll.PayrollId;
    }

    public void Deroll(int employeeId)
    {
        var rows = _sensitive.Payrolls
            .Where(payroll => payroll.EmployeeId == employeeId)
            .ExecuteDelete();

        if (rows == 0)
        {
            throw new InvalidOperationException("employeeId has no payroll");
        }
    }
}