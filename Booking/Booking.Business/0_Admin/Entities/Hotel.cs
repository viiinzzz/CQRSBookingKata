namespace BookingKata.Admin;

public record Hotel(
    string HotelName,

    double Latitude,
    double Longitude,

    int EarliestCheckInTime = 16_00,
    int LatestCheckOutTime = 10_00,

    string LocationAddress = "",
    string ReceptionPhoneNumber = "",
    string url = "",
    int ranking = 2,

    int? ManagerId = default,

    int HotelId = default,
    bool Disabled = false
) 
    : RecordWithValidation, IHavePosition, IHavePrimaryKey
{
    protected override void Validate()
    {
        if (CheckInH is < 0 or > 23)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(EarliestCheckInTime));
        }
        if (CheckOutH is < 0 or > 23)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(LatestCheckOutTime));
        }
        if (CheckInM is < 0 or > 59)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(EarliestCheckInTime));
        }
        if (CheckOutM is < 0 or > 59)
        {
            throw new ArgumentException("must be HHMM (24 hours)", nameof(LatestCheckOutTime));
        }

        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);

    }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long PrimaryKey => HotelId;

    private int CheckInH => EarliestCheckInTime / 100;
    private int CheckInM => EarliestCheckInTime % 100;
    private int CheckOutH => LatestCheckOutTime / 100;
    private int CheckOutM => LatestCheckOutTime % 100;


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore] 
    public double EarliestCheckInHours => CheckInH + CheckInM / 60d;

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore] 
    public double LatestCheckOutHours => CheckOutH + CheckOutM / 60d;


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }
}