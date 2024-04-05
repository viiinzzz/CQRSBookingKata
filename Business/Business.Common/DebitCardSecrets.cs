namespace Business.Common;

public record DebitCardSecrets
(
    string ownerName,
    int expire,
    int CCV
);