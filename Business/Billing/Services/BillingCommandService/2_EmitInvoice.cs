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

public partial class BillingCommandService
{
    public Id<InvoiceRef> EmitInvoice
    (
        double amount,
        string currency,

        int customerId,
        int quotationId,
        long correlationId1,
        long correlationId2
    )
    {
        var now = DateTime.UtcNow;

        var quotation = money.Quotations.FirstOrDefault(q => q.QuotationId == quotationId);

        if (quotation == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(quotationId));
        }

        if (quotation.CorrelationId1 != correlationId1 ||
            quotation.CorrelationId2 != correlationId2)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(Correlation));
        }

        if (now < quotation.OptionStartsUtc &&
            now >= quotation.OptionEndsUtc)
        {
            throw new ArgumentException(ReferenceExpired, nameof(quotationId));
        }


        if (Math.Abs(amount - quotation.Price) >= 0.01)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(amount));
        }

        if (currency != quotation.Currency)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(currency));
        }


        var invoice = new Invoice(
            amount, 
            currency, 
            now,
            customerId,
            quotationId,
            quotation.CorrelationId1,
            quotation.CorrelationId2
         );

        var invoiceId = money.AddInvoice(invoice);

        return new Id<InvoiceRef>(invoiceId);
    }
}