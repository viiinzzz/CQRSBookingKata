namespace Common.Infrastructure.Network;

public record InvoiceRequest
(
    double amount,
    string currency,

    int customerId,
    int quotationId
);