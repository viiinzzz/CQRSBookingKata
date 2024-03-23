namespace CQRSBookingKata.Sales;

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
) : RecordWithValidation
{
    protected override void Validate()
    {
        var coordinatesCount = 0;
        if (Latitude.HasValue) coordinatesCount++;
        if (Longitude.HasValue) coordinatesCount++;

        if (coordinatesCount == 1)
        {
            var argName = string.Empty;
            if (!Latitude.HasValue) argName = nameof(Latitude);
            if (!Longitude.HasValue) argName = nameof(Longitude);

            throw new ArgumentException($"must specify both or none of {nameof(Latitude)}, {nameof(Longitude)}",
                argName);
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
}