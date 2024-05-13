using System.ComponentModel.DataAnnotations.Schema;

namespace BookingKata.Sales;

//room vacancy

public record Vacancy
(
    int DayNum,
    int PersonMaxCount,

    double Latitude,
    double Longitude,

    string HotelName,
    string? CityName,

    bool Cancelled = false,
    int UniqueRoomId = default
)
    : RecordWithValidation, IHavePrimaryKeyAndPosition
{
    public long VacancyId { get; set; } = default;
    
    
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
            // VacancyId = 
            //     UniqueRoomId * 1_0000 +
            //     DayNum; 
            VacancyId = 
                UniqueRoomId +
                DayNum * 1_0000_0000l;
        }

        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);

    }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long PrimaryKey => VacancyId;


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public Position? Position { get; private set; }
    

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

}