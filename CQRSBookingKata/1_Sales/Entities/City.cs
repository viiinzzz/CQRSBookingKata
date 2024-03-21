namespace CQRSBookingKata.Sales;

public record City(string? name = default, string? lat = default, string? lng = default, string? country = default, string? admin1 = default, string? admin2 = default)
{
    double? Latitude
    {
        get
        {

            if (!double.TryParse(lat, out var value))
            {
                return default;
            }

            return value;
        }
    }

    double? Longitude
    {
        get
        {

            if (!double.TryParse(lng, out var value))
            {
                return default;
            }

            return value;
        }
    }
}