namespace Business.Time;

public static class DayNumHelper
{
    public const int YearStart = 2024;

    private static readonly DateTime BaseDate = new(
        YearStart, 1, 1,
        0, 0, 0);

    public static double FractionalDayNum(this DateTime date)
    {
        return (date - BaseDate).TotalDays;
    }
}