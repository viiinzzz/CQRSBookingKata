namespace BookingKata.ThirdParty;

public record PaymentResponse
(
    bool Accepted = default,
    string TransactionTimeUtc = default,
    int TransactionId = default
);