namespace BookingKata.Sales;

public record StayProposition
(

    int PersonCount = default,
    int NightsCount = default,

    string ArrivalDateUtc = default,
    int ArrivalDayNum = int.MaxValue,
    string DepartureDateUtc = default,
    int DepartureDayNum = int.MaxValue,

    double Price = default,
    string Currency = default,

    string OptionStartUtc = default,
    int OptionStartDayNum = int.MinValue,
    string OptionEndUtc = default,
    int OptionEndDayNum = int.MinValue,

    int Urid = default,
    int StayPropositionId = default
)
{
    public bool IsValid(DateTime now)
    {
        var optionStartsUtc = OptionStartUtc.DeserializeUniversal_ThrowIfNull(nameof(OptionStartUtc));
        var optionEndsUtc = OptionEndUtc.DeserializeUniversal_ThrowIfNull(nameof(OptionEndUtc));

        return now >= optionStartsUtc &&
               now < optionEndsUtc;
    }
}