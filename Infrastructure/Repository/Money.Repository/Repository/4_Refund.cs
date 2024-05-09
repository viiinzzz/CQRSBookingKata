namespace Infrastructure.Storage;

public partial class MoneyRepository
{
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