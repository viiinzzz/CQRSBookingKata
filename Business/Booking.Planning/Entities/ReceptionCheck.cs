namespace BookingKata.Planning;

public record ReceptionCheck
(
    int EventDayNum = default,
    DateTime EventTime = default,
    ReceptionEventType EventType = default,

    string CustomerLastName = default,
    string CustomerFirstName = default,

    int RoomNum = default,
    int FloorNum = default,
    int HotelId = default,
    double Latitude = default,
    double Longitude = default,

    int BookingId = default,
    int? EmployeeId = default,
    bool TaskDone = default,
    bool Cancelled = default,
    DateTime? CancelledDate = default,
    int CheckId = default
) 
    : RecordWithValidation, IHavePrimaryKeyAndPosition
{
    protected override void Validate()
    {
        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);
    }

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long PrimaryKey => CheckId;


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }
}
