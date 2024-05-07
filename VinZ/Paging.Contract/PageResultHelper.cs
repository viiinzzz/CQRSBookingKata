namespace VinZ.Common;

public static class PageResultHelper
{
    public static IEnumerable<TEntity> GetCollection<TEntity>(this PageResult<TEntity> pageResult)
        where TEntity : class
    {
        return pageResult.items?.AsEnumerable()
               ?? Enumerable.Empty<TEntity>();
    }

    public static void SetCollection<TEntity>(this PageResult<TEntity> pageResult, IEnumerable<TEntity> items)
        where TEntity : class
    {
        var previousItemsLength = pageResult.items?.Length;

        pageResult.items = items.ToArray();

        if (previousItemsLength.HasValue &&
            pageResult.items.Length != previousItemsLength)
        {
            throw new ArgumentException("must be same Count() as previously", nameof(items));
        }
    }
}