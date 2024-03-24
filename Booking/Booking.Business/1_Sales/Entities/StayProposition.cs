namespace CQRSBookingKata.Sales;

public record StayProposition
(
    int PersonCount,
    DateTime ArrivalDate,
    DateTime DepartureDate,

    double Price,
    string Currency,

    DateTime? OptionStartsUtc,
    DateTime? OptionEndsUtc,

    int Urid,
    int StayPropositionId = 0
)
{
    public bool IsValid(DateTime now)
        
        => now >= OptionStartsUtc && now < OptionEndsUtc;
}