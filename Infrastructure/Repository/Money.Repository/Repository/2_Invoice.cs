namespace Infrastructure.Storage;

public partial class MoneyRepository
{
    public IQueryable<Invoice> Invoices

        => _money.Invoices
            .AsNoTracking();

    public int AddInvoice(Invoice invoice)
    {
        _money.Invoices.Add(invoice);
        _money.SaveChanges();
        _money.Entry(invoice).State = EntityState.Detached;

        return invoice.InvoiceId;
    }

    public void CancelInvoice(int invoiceId)
    {
        var invoice = _money.Invoices.Find(invoiceId);

        if (invoice == default)
        {
            throw new InvalidOperationException(ReferenceInvalid);
        }

        _money.Entry(invoice).State = EntityState.Detached;

        invoice = invoice with { Cancelled = true };

        var entity = _money.Invoices.Update(invoice);
        _money.SaveChanges();
        entity.State = EntityState.Detached;
    }
}