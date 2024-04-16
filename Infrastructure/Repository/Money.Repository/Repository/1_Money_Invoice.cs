namespace Infrastructure.Storage;

public partial class MoneyRepository
{

    public IQueryable<Quotation> Quotations

        => _money.Quotations
            .AsNoTracking();

    public int AddQuotation(Quotation quotation)
    {
        _money.Quotations.Add(quotation);
        _money.SaveChanges();
        _money.Entry(quotation).State = EntityState.Detached;

        return quotation.QuotationId;
    }

    public void UpdateQuotation(int quotationId, Quotation quotationUpdate)
    {
        if (quotationId != quotationUpdate.QuotationId)
        {
            throw new ArgumentException(ReferenceMismatch, nameof(quotationId));
        }

        var quotation = _money.Quotations
            .Find(quotationId);

        if (quotation == default)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }

        if (quotation.ReferenceId != quotationUpdate.ReferenceId)
        {
            throw new ArgumentException(ReferenceMismatch, nameof(quotationUpdate.ReferenceId));
        }


        _money.Entry(quotation).State = EntityState.Detached;

        
        var entity = _money.Quotations.Update(quotationUpdate);
        _money.SaveChanges();
        entity.State = EntityState.Detached;
    }

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


    public IQueryable<Receipt> Receipts

        => _money.Receipts
            .AsNoTracking();

    public int AddReceipt(Receipt receipt)
    {
        _money.Receipts.Add(receipt);
        _money.SaveChanges();
        _money.Entry(receipt).State = EntityState.Detached;

        return receipt.ReceiptId;
    }

    public IQueryable<Refund> Refunds

        => _money.Refunds
            .AsNoTracking();

    public int AddRefund(Refund refund)
    {
        _money.Refunds.Add(refund);
        _money.SaveChanges();
        _money.Entry(refund).State = EntityState.Detached;

        return refund.RefundId;
    }

}