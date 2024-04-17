namespace Business.Common;

public static class DayNumHelper
{
    public const int YearStart = 2024;

    private static readonly DateTime BaseDate = new(
        YearStart, 1, 1,
        0, 0, 0);

    private static readonly int FractionalDayNumEpsilonInverse = // minute epsilon
        (int)Math.Round(1 / TimeSpan.FromMinutes(1).TotalDays);

    public static double FractionalDayNum(this DateTime date)
    {
        var value = (date - BaseDate).TotalDays;

        value = Math.Round(value * FractionalDayNumEpsilonInverse) / FractionalDayNumEpsilonInverse;

        return value;
    }
}