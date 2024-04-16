namespace Common.Infrastructure.Network;

public record PaymentRequest
(
    long debitCardNumber = default,
    string debitCardOwnerName = default,
    int debitCardExpire = default,
    int debitCardCCV = default,

    int vendorId = default,
    int terminalId = default,

    int invoiceId = default
);