namespace BookingKata.Billing;

public record Receipt
(
    long DebitCardNumber,
    double Amount,
    string Currency,

    DateTime SettledTime,

    int CustomerId,
    int InvoiceId,

    long CorrelationId1,
    long CorrelationId2,
    int ReceiptId = default
)
{
    public Invoice Invoice { get; set; }
}