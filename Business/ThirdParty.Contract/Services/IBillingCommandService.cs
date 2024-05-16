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

public interface IBillingCommandService
{
    Id<QuotationRef> EmitQuotation
    (
        double price,
        string currency,
        DateTime optionStartUtc,
        DateTime optionEndUtc,
        string jsonMeta,

        int referenceId,
        long correlationId1,
        long correlationId2
    );

    Id<InvoiceRef> EmitInvoice
    (
        double amount,
        string currency,

        int customerId,
        int quotationId,
        long correlationId1,
        long correlationId2
    );


    Id<ReceiptRef> EmitReceipt
    (
        double amount,
        string currency,

        long debitCardNumber,
        DebitCardSecrets secrets,
        VendorIdentifiers vendor,

        int invoiceId,
        long correlationId1,
        long correlationId2
    );

    Id<RefundRef> EmitRefund
    (
        int receiptId,
        long correlationId1,
        long correlationId2
    );

    Id<PayrollRef> EmitPayroll
    (
        int employeeId,
        double monthlyIncome,
        string currency,

        long correlationId1,
        long correlationId2
    );
}