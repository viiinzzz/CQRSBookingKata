namespace BookingKata.Billing;

public partial class BillingCommandService
{
    public int EmitQuotation
    (
        double price,
        string currency,
        DateTime? optionStartUtc,
        DateTime? optionEndUtc,
        string jsonMeta,

        int referenceId,
        long correlationId1,
        long correlationId2
    )
    {
        var quotation = new Quotation
        (
            price,
            currency,
            optionStartUtc,
            optionEndUtc,
            jsonMeta,
            referenceId,
            correlationId1,
            correlationId2
        );

        var quotationId = money.AddQuotation(quotation);

        return quotationId;
    }
}