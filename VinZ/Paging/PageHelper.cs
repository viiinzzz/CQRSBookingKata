namespace VinZ.Paging;

public static class PageHelper
{
    private static readonly int DefaultPageSize = 50;


    public static PageResult<TEntity> Page<TEntity>
    (
        this IQueryable<TEntity> query,
        string baseUrl,
        int? pageSpec,
        int? pageSizeSpec,
        Action<IQueryable<TEntity>>? queryCheck// = default
    )
        where TEntity : class
    {
        var page = pageSpec;
        if (page is null or < 1) page = 1;

        var page0 = page - 1;

        var pageSize = pageSizeSpec ?? DefaultPageSize;

        var pageQuery = query
            .Skip((page0 ?? 0) * pageSize)
            .Take(pageSize);

        queryCheck?.Invoke(pageQuery);

        //
        //
        var itemCount = query.Count();
        //
        //

        var pageCount = itemCount == 0 ? 0 : 1 + (itemCount / pageSize);

        if (page > pageCount) page = pageCount;



        //
        //
        var items = pageQuery
            .AsEnumerable();
        //
        //

        var elementType = typeof(TEntity).Name;
        var type = $"{elementType}Collection";

        var links = new PageLinks(baseUrl, page.Value, pageSize, pageCount);

        return new PageResult<TEntity>(
                page.Value, pageSize, pageCount, itemCount,
                new[] { links },
                type, elementType
                ) { Collection = items };
    }

}