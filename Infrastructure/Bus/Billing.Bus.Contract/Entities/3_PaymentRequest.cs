namespace Support.Infrastructure.Network;

public record PaymentRequest
(
    int referenceId = default,

    double amount = default,
    string currency = default,

    long debitCardNumber = default,
    string debitCardOwnerName = default,
    int debitCardExpire = default,
    int debitCardCCV = default,

    int vendorId = default,
    int terminalId = default
);
