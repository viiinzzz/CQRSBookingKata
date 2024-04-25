namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public Id EmitQuotation
    (
        double price,
        string currency,
        DateTime optionStartUtc,
        DateTime optionEndUtc,
        string jsonMeta,

        int referenceId,
        long correlationId1,
        long correlationId2
    )
    {
        var previousQuotation = money.Quotations
            .FirstOrDefault(quotation => quotation.ReferenceId == referenceId);

        var quotation = new Quotation
        {
            Price = price,
            Currency = currency,
            OptionStartsUtc = optionStartUtc,
            OptionEndsUtc = optionEndUtc,
            jsonMeta = jsonMeta,
            ReferenceId = referenceId,
            VersionNumber = previousQuotation == null ? 1 : previousQuotation.VersionNumber + 1,
            CorrelationId1 = correlationId1,
            CorrelationId2 = correlationId2
        };

        if (previousQuotation == null)
        {
            return new Id(money.AddQuotation(quotation));
        }

        var quotationId = previousQuotation.QuotationId;

        if (previousQuotation with { VersionNumber = 0 } == quotation with { VersionNumber = 0 })
        {
            return new Id(quotationId);
        }

        money.UpdateQuotation(quotationId, quotation);

        return new Id(quotationId);
    }
}