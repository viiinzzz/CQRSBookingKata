namespace VinZ.Common;

public static class EnumerableHelper
{
    public static IEnumerable<int> RangeTo(this int from, int to)
    {
        return Enumerable.Range(from, to - from + 1);
    }

    private static System.Random Random = new System.Random();

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