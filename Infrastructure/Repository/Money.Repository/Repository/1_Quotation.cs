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
}