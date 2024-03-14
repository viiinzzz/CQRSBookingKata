namespace CQRSBookingKata.Billing;

public interface IBillingRepository
{
    IQueryable<Booking> AllBookings { get; }

    void AddBookingAndInvoice(Booking booking, Invoice invoice);
}