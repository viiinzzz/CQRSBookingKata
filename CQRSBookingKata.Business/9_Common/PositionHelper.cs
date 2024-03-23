namespace CQRSBookingKata.Common;

public static class PositionHelper
{
    public static double ArcDist(this Position p1, Position p2, double radius)
    {

        var lat = DegToRad(p1.Latitude);
        var lon = DegToRad(p2.Latitude);
        var dLat = DegToRad(p2.Latitude - p1.Latitude);
        var dLon = DegToRad(p2.Longitude - p1.Longitude);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat) * Math.Cos(lon);

        var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));

        var d = radius * c;

        return d;
    }

    private static double DegToRad(this double val)
    {
        return (Math.PI / 180) * val;
    }
}