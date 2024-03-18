namespace CQRSBookingKata.Sales;

//room vacancy

public record Vacancy
(
    int DayNum,
    int PersonMaxCount,

    double Latitude,
    double Longitude,

    int Urid
)
    : RecordWithValidation
{
    public long VacancyId { get; set; } = 0;
    
    
    protected override void Validate()
    {
        if (Urid is < 1 or > 9999_9999 or 1_0000)
        {
            throw new ArgumentException("value must be at least 1 and at most 9999 9999, not 1 0000", nameof(Urid));
        }
        if (DayNum is < 0 or > 9999)
        {
            throw new ArgumentException("value must be at least 0 and at most 9999", nameof(DayNum));
        }
        if (PersonMaxCount is < 0 or > 9)
        {
            throw new ArgumentException("value must be at least 0 and at most 9", nameof(PersonMaxCount));
        }
    
        if (VacancyId == 0)
        {
            VacancyId = 
                Urid * 10000 +
                DayNum;
        }
    }
}