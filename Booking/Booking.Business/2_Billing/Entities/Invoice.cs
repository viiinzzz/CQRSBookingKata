namespace BookingKata.Billing;

public record Invoice(
    long DebitCardNumber,
    double Amount,
    string Currency,

    int CustomerId,
    int InvoiceId = default
);