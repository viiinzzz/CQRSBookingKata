namespace CQRSBookingKata.Billing;

public interface IBillingRepository
{
    IQueryable<Booking> AllBookings { get; }

    void AddBookingAndInvoice(Booking booking, Invoice invoice);


    public int Enroll(int employeeId, double monthlyBaseIncome, string currency);

    public void Deroll(int employeeId);
}