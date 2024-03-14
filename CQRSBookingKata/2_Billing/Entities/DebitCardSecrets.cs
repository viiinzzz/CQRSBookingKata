namespace CQRSBookingKata.Billing;

public record DebitCardSecrets(
    string ownerName,
    int expire,
    int CCV
);