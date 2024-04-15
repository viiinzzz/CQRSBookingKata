namespace BookingKata.Planning;

public record ReceptionCheck
(
    int EventDayNum,
    DateTime EventTime,
    ReceptionEventType EventType,

    string CustomerLastName,
    string CustomerFirstName,

    int RoomNum,
    bool TaskDone,

    double Latitude,
    double Longitude,

    int HotelId,
    int BookingId,
    
    DateTime? CancelledDate,
    bool Cancelled,
    
    int? EmployeeId,
    int CheckId
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
