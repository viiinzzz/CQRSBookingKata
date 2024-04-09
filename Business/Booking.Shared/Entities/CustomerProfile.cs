namespace BookingKata.Shared;


public record CustomerProfile(int CustomerId)
{
    public IList<Booked> BookedHistory { get; set; }
}


public record Booked
(
    DateTime ArrivalDate = default,
    DateTime DepartureDate = default,

    double Latitude = default,
    double Longitude = default,

    string HotelName = default,
    string CityName = default,

    int PersonCount = default,

    double Price = default,
    string Currency = default,

    int UniqueRoomId = default,
    bool Cancelled = false
)
: RecordWithValidation, IHavePosition
{

    protected override void Validate()
    {
        Position =
            this is { Latitude: 0, Longitude: 0 }
                ? default
                : new Position(Latitude, Longitude);

    }

    [System.Text.Json.Serialization.JsonIgnore]
    // [Newtonsoft.Json.JsonIgnore]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    // [Newtonsoft.Json.JsonIgnore]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

}
