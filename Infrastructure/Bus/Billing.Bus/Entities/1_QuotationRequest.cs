namespace Common.Infrastructure.Network;

public record QuotationRequest
(
    double price,
    string currency,

    string? optionStartUtc,
    string? optionEndUtc,

    string? jsonMeta,

    int referenceId
);