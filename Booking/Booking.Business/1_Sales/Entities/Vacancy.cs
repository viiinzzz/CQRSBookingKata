namespace CQRSBookingKata.Sales;

//room vacancy

public record Vacancy
(
    int DayNum,
    int PersonMaxCount,

    double Latitude,
    double Longitude,
    long? Level12,
    long? Level11,
    long? Level10,
    long? Level9,
    long? Level8,
    long? Level7,
    long? Level6,
    long? Level5,
    long? Level4,
    long? Level3,
    long? Level2,
    long? Level1,
    long? Level0,

    string HotelName,
    string CityName,

    bool Cancelled = false,
    int UniqueRoomId = 0
)
    : RecordWithValidation
{
    public long VacancyId { get; set; } = 0;
    
    
    protected override void Validate()
    {
        if (UniqueRoomId is < 1 or >= 1_0000_0000 or 1_0000)
        {
            throw new ArgumentException("value must be at least 1 and at most 9999 9999, not 1 0000", nameof(UniqueRoomId));
        }
        if (DayNum is < 0 or >= 1_0000)
        {
            throw new ArgumentException("value must be at least 0 and at most 9999", nameof(DayNum));
        }
        if (PersonMaxCount is < 0 or >= 10)
        {
            throw new ArgumentException("value must be at least 0 and at most 9", nameof(PersonMaxCount));
        }
    
        if (VacancyId == 0)
        {
            VacancyId = 
                UniqueRoomId * 1_0000 +
                DayNum;
        }
    }
}