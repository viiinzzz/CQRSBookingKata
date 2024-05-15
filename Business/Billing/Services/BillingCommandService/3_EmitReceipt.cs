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
    public Id EmitReceipt
    (
        double amount,
        string currency,

        long debitCardNumber,
        DebitCardSecrets secrets,
        VendorIdentifiers vendor,
        
        int invoiceId,
        long correlationId1,
        long correlationId2
    )
    {
        var now = DateTime.UtcNow;

        var invoice = money.Invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);

        if (invoice == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(invoiceId));
        }

        if (invoice.CorrelationId1 != correlationId1 ||
            invoice.CorrelationId2 != correlationId2)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(Correlation));
        }

        var originator = GetType().FullName 
                         ?? throw new ArgumentException("invalid originator");

        //
        //
        var paid = bus.AskResult<PaymentResponse>(Support.Services.ThirdParty.Recipient, Support.Services.ThirdParty.Verb.RequestPayment,
            new PaymentOrder
            {
                amount = invoice.Amount,
                currency = invoice.Currency,

                debitCardNumber = debitCardNumber,
                debitCardOwnerName = secrets.ownerName,
                expire = secrets.expire,
                CCV = secrets.CCV,

                vendorId = vendor.vendorId,
                terminalId = vendor.terminalId,
                transactionId = invoiceId
            }, originator);
        //
        //

        if (paid is not { Accepted: true })
        {
            throw new PaymentFailureException();
        }

        var receipt = new Receipt(
            debitCardNumber,
            invoice.Amount, 
            invoice.Currency,
            now, 
            invoice.CustomerId,
            invoiceId,
            correlationId1,
            correlationId2
        );
        
        var receiptId = money.AddReceipt(receipt);

        return new Id(receiptId);
    }
}