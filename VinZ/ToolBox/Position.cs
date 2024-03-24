using Google.Common.Geometry;

namespace VinZ.ToolBox;

public record struct Position(double Latitude, double Longitude)
{

    // public const double EarthRadius = 6371d;
    // public const double EarthRadiusKm = S2LatLng.EarthRadiusMeters / 1000;
    //
    // public bool IsEarthMatch(double latitude, double longitude, int maxKm)
    // {
    //     var p1 = this;
    //     var p2 = new Position(latitude, longitude);
    //
    //     return p1 - p2 < maxKm;
    // }
    //
    // public static double EarthKm(Position p1, Position p2)
    // {
    //     return p1.ArcDist(p2, EarthRadiusKm);
    // }
    //
    // public static double operator -(Position p1, Position p2)
    // {
    //     return EarthKm(p1, p2);
    // }

}