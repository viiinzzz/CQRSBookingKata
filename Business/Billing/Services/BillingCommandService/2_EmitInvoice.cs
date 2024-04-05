﻿
namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public int EmitInvoice
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
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(quotationId));
        }

        if (quotation.CorrelationId1 != correlationId1 ||
            quotation.CorrelationId2 != correlationId2)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceInvalid, nameof(Correlation));
        }

        if (now < quotation.OptionStartsUtc &&
            now >= quotation.OptionEndsUtc)
        {
            throw new ArgumentException(Common.Exceptions.ReferenceExpired, nameof(quotationId));
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

        return invoiceId;
    }
}