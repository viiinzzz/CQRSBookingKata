namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public Id EmitInvoice
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

        return new Id(invoiceId);
    }
}