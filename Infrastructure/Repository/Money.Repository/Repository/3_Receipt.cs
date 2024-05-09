namespace Infrastructure.Storage;

public partial class MoneyRepository
{
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
}