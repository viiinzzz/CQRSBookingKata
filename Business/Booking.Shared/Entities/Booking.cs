namespace BookingKata.Shared;

public record Booking
(
    DateTime ArrivalDate = default,
    DateTime DepartureDate = default,
    int NightsCount = default,

    double Latitude = default,
    double Longitude = default,

    string HotelName = default,
    string CityName = default,

    string LastName = default,
    string FirstName = default,

    int PersonCount = default,

    double Price = default,
    string Currency = default,

    int UniqueRoomId = default,
    int CustomerId = default,
    int BookingId = default,

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