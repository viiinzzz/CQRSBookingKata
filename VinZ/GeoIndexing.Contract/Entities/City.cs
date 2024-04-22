
namespace VinZ.GeoIndexing;

public record City
(
    string? name = default, 
    string? lat = default,
    string? lng = default,
    string? country = default,
    string? admin1 = default,
    string? admin2 = default
)
    : RecordWithValidation, IHavePosition
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

    protected override void Validate()
    {
        Position = GetPosition();
    }


    [System.Text.Json.Serialization.JsonIgnore]
    // [Newtonsoft.Json.JsonIgnore]
    public Position? Position { get; private set; }


    [System.Text.Json.Serialization.JsonIgnore]
    public IList<IGeoIndexCell> Cells { get; set; }
    public string geoIndex { get; set; }

}
