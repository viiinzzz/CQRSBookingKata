
using CQRSBookingKata.API.Databases;
using CQRSBookingKata.Billing;
using CQRSBookingKata.Sales;

namespace CQRSBookingKata.API.Repositories;

public class BillingRepository(BookingBackContext back, BookingSensitiveContext sensitive) : IBillingRepository
{
    public IQueryable<Booking> AllBookings => back.Bookings.AsQueryable();

    public void AddBookingAndInvoice(Booking booking, Invoice invoice)
    {

        using var transaction1 = sensitive.Database.BeginTransaction();

        try
        {
            back.Bookings.Add(booking);
            back.SaveChanges();


            sensitive.Invoices.Add(invoice);
            sensitive.SaveChanges();
            
            transaction1.Commit();
        }
        catch (Exception e)
        {
            transaction1.Rollback();

            throw new ServerErrorException(e);
        }

    }
}