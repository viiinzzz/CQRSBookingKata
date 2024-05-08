namespace BookingKata.Billing;

public interface IMoneyRepository
{
    IQueryable<Quotation> Quotations { get; }
    int AddQuotation(Quotation quotation);
    void UpdateQuotation(int quotationId, Quotation quotationUpdate);

    IQueryable<Invoice> Invoices { get; }
    int AddInvoice(Invoice invoice);
    void CancelInvoice(int invoiceId);

    IQueryable<Receipt> Receipts { get; }
    int AddReceipt(Receipt receipt);

    IQueryable<Refund> Refunds { get; }
    int AddRefund(Refund refund);


    IQueryable<Payroll> Payrolls { get; }
    int EnrollEmployee(int employeeId, double monthlyBaseIncome, string currency);
    void Deroll(int employeeId);
}