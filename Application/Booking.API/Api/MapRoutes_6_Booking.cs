namespace BookingKata.API;

public static partial class ApiMethods
{
    private const string PublicTag = "Public";
    private const string BookingTag = "Booking";
    
    private static void MapRoutes_6_Booking(WebApplication app)
    {
        var pattern = "/booking";

        app.MapGet(pattern,
            (
                int? page, int? pageSize,
                [FromQuery(Name = "arrival")] DateTime arrivalDate,
                [FromQuery(Name = "departure")] DateTime departureDate,
                [FromQuery(Name = "persons")] int personCount,
                [FromQuery(Name = "approx")] bool ? approximateNameMatch,
                [FromQuery(Name = "hotel")] string ? hotelName,
                [FromQuery(Name = "country")] string ? countryCode,
                [FromQuery(Name = "city")] string ? cityName,
                [FromQuery(Name = "lat")] ParsableNullableDouble latitude,
                [FromQuery(Name = "lon")] ParsableNullableDouble longitude,
                [FromQuery(Name = "km")] ParsableNullableInt maxKm,
                [FromQuery(Name = "pricemin")] ParsableNullableInt priceMin,
                [FromQuery(Name = "pricemax")] ParsableNullableInt priceMax,
                [FromQuery(Name = "currency")] string ? currency,
                [FromServices] SalesQueryService sales
            )
                =>
            {
                var filter = new StayRequest(personCount, arrivalDate, departureDate) {
                    ApproximateNameMatch = approximateNameMatch,
                    HotelName = hotelName,
                    CountryCode =  countryCode,
                    CityName = cityName,

                    Latitude = latitude.Value,
                    Longitude = longitude.Value,
                    MaxKm = maxKm.Value,

                    PriceMin = priceMin.Value,
                    PriceMax = priceMax.Value,
                    Currency =  currency
                };

                return originator.ListMq<StayProposition>(
                    Recipient.Sales, Verb.Sales.RequestStay,
                    pattern, pattern, filter, responseTimeoutSeconds);
            }).WithOpenApi().WithTags([PublicTag, BookingTag]);

    }
}