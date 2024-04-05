namespace BookingKata.Billing;

public record Refund
(
    long DebitCardNumber,
    double Amount,
    string Currency,

    DateTime RefundTime,

    int CustomerId,
    int InvoiceId,

    long CorrelationId1,
    long CorrelationId2,
    int RefundId = default
)
{
    public Invoice Invoice { get; set; }
}