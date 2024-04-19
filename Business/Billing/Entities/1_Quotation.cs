namespace BookingKata.Billing;

public record Quotation
(
    double Price = default,
    string Currency = default,

    DateTime OptionStartsUtc = default,
    DateTime OptionEndsUtc = default,

    string jsonMeta = default,

    int ReferenceId = default,
    int VersionNumber = default,

    long CorrelationId1 = default,
    long CorrelationId2 = default,
    int QuotationId = default
);