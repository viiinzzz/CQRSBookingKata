namespace VinZ.ToolBox;

public static class EnumerableHelper
{
    public static IEnumerable<int> RangeTo(this int from, int to)
    {
        return Enumerable.Range(from, to - from + 1);
    }

    private static Random Random = new Random();

    public static IEnumerable<T> AsRandomEnumerable<T>(this IQueryable<T> collection)
        => AsRandomEnumerable(collection.AsEnumerable());

    public static IEnumerable<T> AsRandomEnumerable<T>(this IEnumerable<T> collection)
    {
        var scrambled = collection
            .OrderBy(item => Random.Next());

        foreach (var item in scrambled)
        {
            yield return item;
        }
    }
}

public static class DateTimeHelper
{
    public static DateTime DayStart(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
    }
}