using System.ComponentModel.DataAnnotations.Schema;

namespace BookingKata.Shared;

public record Booking
(
    DateTime ArrivalDate = default,
    DateTime DepartureDate = default,
    int ArrivalDayNum = default,
    int DepartureDayNum = default,
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

    int RoomNum = default,
    int FloorNum = default,
    int HotelId = default,

    int UniqueRoomId = default,
    int CustomerId = default,
    int BookingId = default,
    bool Cancelled = false
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
    // [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    // [Newtonsoft.Json.JsonIgnore]
    [NotMapped]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

    public long PrimaryKey => BookingId;
}