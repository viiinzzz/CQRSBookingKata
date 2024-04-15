namespace Common.Infrastructure.Network;

public record PaymentOrder
(
    double amount = default,
    string currency = default,

    long debitCardNumber = default,
    string debitCardOwnerName = default,
    int expire = default,
    int CCV = default,

    int vendorId = default,
    int terminalId = default,
    int transactionId = default
);