/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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