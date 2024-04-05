namespace BookingKata.Billing;

public record Invoice
(
    double Amount,
    string Currency,

    DateTime EmitTime,

    int CustomerId,
    int QuotationId,

    long CorrelationId1,
    long CorrelationId2,
    bool Cancelled = false,
    int InvoiceId = default
)
{
    public Quotation Quotation { get; set; }
    public Receipt? Receipt { get; set; }
}