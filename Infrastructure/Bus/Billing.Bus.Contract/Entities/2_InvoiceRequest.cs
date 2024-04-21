namespace Support.Infrastructure.Network;

public record InvoiceRequest
(
    double amount = default,
    string currency = default,

    int customerId = default,
    int quotationId = default
);