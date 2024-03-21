namespace CQRSBookingKata.Billing;

public interface IMoneyRepository
{
    void AddInvoice(Invoice invoice, bool scoped);
    int Enroll(int employeeId, double monthlyBaseIncome, string currency);
    void Deroll(int employeeId);
}