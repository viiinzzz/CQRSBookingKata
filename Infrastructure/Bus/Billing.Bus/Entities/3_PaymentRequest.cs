namespace BookingKata.Infrastructure.Network;

public record PaymentRequest
(
    long debitCardNumber,
    string ownerName,
    int expire,
    int CCV,

    int invoiceId
);