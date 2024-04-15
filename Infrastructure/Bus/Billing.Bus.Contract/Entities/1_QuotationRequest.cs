namespace Common.Infrastructure.Network;

public record QuotationRequest
(
    double price = default,
    string currency = default,

    string? optionStartUtc = default,
    string? optionEndUtc = default,

    string? jsonMeta = default,

    int referenceId = default
);