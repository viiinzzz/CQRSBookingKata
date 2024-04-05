namespace VinZ.Common;


public static class PositionHelper
{
    public static double ArcDist(this Position p1, Position p2, double radius)
    {
        //Haversine

        var lat = p1.Latitude.DegToRad();
        var lon = p2.Latitude.DegToRad();
        var dLat = (p2.Latitude - p1.Latitude).DegToRad();
        var dLon = (p2.Longitude - p1.Longitude).DegToRad();

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat) * Math.Cos(lon);

        var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));

        var d = radius * c;

        return d;
    }

    public static double DegToRad(this double val)
    {
        return Math.PI / 180 * val;
    }
}
