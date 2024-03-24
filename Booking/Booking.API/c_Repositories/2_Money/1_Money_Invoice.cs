namespace CQRSBookingKata.API;

public partial class MoneyRepository
{

    public IQueryable<Invoice> Invoices

        => _money.Invoices
            .AsNoTracking();


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
}