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
    public Id<RefundRef> EmitRefund
    (
        int receiptId,
        long correlationId1,
        long correlationId2
    )
    {
        var now = DateTime.UtcNow;

        var receipt = money.Receipts.FirstOrDefault(r => r.ReceiptId == receiptId);

        if (receipt == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(receiptId));
        }

        if (receipt.CorrelationId1 != correlationId1 ||
            receipt.CorrelationId2 != correlationId2)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(Correlation));
        }


        var refund = new Refund(
            receipt.DebitCardNumber,
            receipt.Amount,
            receipt.Currency,
            now, 
            receipt.CustomerId,
            receipt.InvoiceId,
            receipt.CorrelationId1,
            receipt.CorrelationId2
         );

        var refundId = money.AddRefund(refund);

        return new Id<RefundRef>(refundId);
    }
}