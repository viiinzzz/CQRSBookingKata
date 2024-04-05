namespace BookingKata.Infrastructure.Bus.Sales;

public record BookRequest(
    int? arrivalTime,
    int? departureTime,

    int stayPropositionId
);