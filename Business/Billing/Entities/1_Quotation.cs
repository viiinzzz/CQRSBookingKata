namespace BookingKata.Billing;

public record Quotation
(
    double Price,
    string Currency,

    DateTime? OptionStartsUtc,
    DateTime? OptionEndsUtc,

    string jsonMeta,

    int ReferenceId,

    long CorrelationId1,
    long CorrelationId2,
    int QuotationId = default
);