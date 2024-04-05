namespace BookingKata.Billing;

public interface IMoneyRepository
{
    IQueryable<Quotation> Quotations { get; }
    int AddQuotation(Quotation quotation);

    IQueryable<Invoice> Invoices { get; }
    int AddInvoice(Invoice invoice);
    void CancelInvoice(int invoiceId);

    IQueryable<Receipt> Receipts { get; }
    int AddReceipt(Receipt receipt);

    IQueryable<Refund> Refunds { get; }
    int AddRefund(Refund refund);


    IQueryable<Payroll> Payrolls { get; }
    int Enroll(int employeeId, double monthlyBaseIncome, string currency);
    void Deroll(int employeeId);
}