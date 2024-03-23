namespace CQRSBookingKata.Common;

public record struct Position(double Latitude, double Longitude)
{

    public const double EarthRadius = 6371d;

    public bool IsEarthMatch(double latitude, double longitude, int maxKm)
    {
        var p1 = this;
        var p2 = new Position(latitude, longitude);

        return (p1 - p2) < maxKm;
    }

    public static double EarthKm(Position p1, Position p2)
    {
        return p1.ArcDist(p2, EarthRadius);
    }

    public static double operator -(Position p1, Position p2)
    {
        return EarthKm(p1, p2);
    }
}