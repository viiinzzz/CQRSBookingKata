namespace CQRSBookingKata.Billing;

public interface IMoneyRepository
{
    IQueryable<Invoice> Invoices { get; }
    void AddInvoice(Invoice invoice, bool scoped);

    IQueryable<Payroll> Payrolls { get; }
    int Enroll(int employeeId, double monthlyBaseIncome, string currency);
    void Deroll(int employeeId);
}