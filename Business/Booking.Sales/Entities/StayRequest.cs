namespace BookingKata.Sales;

public record StayRequest
(
    DateTime ArrivalDate,
    DateTime DepartureDate,
    int PersonCount,

    bool? ApproximateNameMatch = default,
    string? HotelName = default,
    string? CountryCode = default,
    string? CityName = default,

    double? Latitude = default,
    double? Longitude = default,
    int? MaxKm = default,

    double? PriceMin = default,
    double? PriceMax = default,
    string? Currency = default
) : RecordWithValidation, IHavePrimaryKeyAndPosition
{
    protected override void Validate()
    {
        var coordinatesCount = 0;
        if (Latitude.HasValue) coordinatesCount++;
        if (Longitude.HasValue) coordinatesCount++;

        switch (coordinatesCount)
        {
            case 1:
            {
                var argName = string.Empty;
                if (!Latitude.HasValue) argName = nameof(Latitude);
                if (!Longitude.HasValue) argName = nameof(Longitude);

                throw new ArgumentException($"must specify both or none of {nameof(Latitude)}, {nameof(Longitude)}",
                    argName);
            }
            case 2:
                Position = new Position(Latitude!.Value, Longitude!.Value);
                break;
        }

        if (DepartureDate < ArrivalDate)
        {
            throw new ArgumentException($"must be later than {nameof(ArrivalDate)}", nameof(DepartureDate));
        }

        if (ArrivalDate.Year == DepartureDate.Year && ArrivalDate.Month == DepartureDate.Month && ArrivalDate.Day == DepartureDate.Day)
        {
            throw new ArgumentException($"must be different of {nameof(ArrivalDate)}", nameof(DepartureDate));
        }

        if (PersonCount is < 1 or > 9)
        {
            throw new ArgumentException("must be 1 or greater and at most 9", nameof(PersonCount));
        }
    }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

}