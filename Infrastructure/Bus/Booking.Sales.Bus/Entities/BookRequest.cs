namespace BookingKata.Infrastructure.Bus.Sales;

public record BookRequest
(
    string lastName,
    string firstName,

    long debitCardNumber,
    string debitCardOwner,
    int debitCardExpire,
    int debitCardCCV,

    int vendorId,
    int terminalId,

    int arrivalTime,
    int departureTime,

    int customerId,
    int stayPropositionId
);