namespace CQRSBookingKata.Sales;

public record City(string? name = default, string? lat = default, string? lng = default, string? country = default, string? admin1 = default, string? admin2 = default)
:RecordWithValidation
{
    private double? GetLatitude()
    {
        if (!double.TryParse(lat, out var value)
            || value > 90 || value < -90)
        {
            return default;
        }

        return value;
    }

    private double? GetLongitude()
    {
        if (!double.TryParse(lng, out var value)
            || value > 180 || value < -180)
        {
            return default;
        }

        return value;
    }

    private Position? GetPosition()
    {
        var latitude = GetLatitude();
        if (!latitude.HasValue)
        {
            return default;
        }

        var longitude = GetLongitude();
        if (!longitude.HasValue)
        {
            return default;
        }
        
        return new Position(latitude.Value, longitude.Value);
    }

    public Position? Position;
    public CellId[]? Cells;

    protected override void Validate()
    {
        Position = GetPosition();

        if (Position != default)
        {
            Cells = Position.Value.CellIds();
        }
    }
}
