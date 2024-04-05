namespace VinZ.Common;

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
        var elementType = typeof(TEntity).Name;
        var type = $"{elementType}Collection";

        var page = pageSpec;
        if (page is null or < 1) page = 1;

        var page0 = page - 1;

        var pageSize = pageSizeSpec ?? DefaultPageSize;

        var pageQuery = query
            .Skip((page0 ?? 0) * pageSize)
            .Take(pageSize);

        try
        {
            queryCheck?.Invoke(pageQuery);
        }
        catch (Exception ex)
        {
            return new PageResult<TEntity>
            (
                type, elementType,
                page.Value, pageSize,
                error: true, reason: ex.Message,
                0, 0, Array.Empty<PageLinks>()
            )
            {
                Collection = Enumerable.Empty<TEntity>()
            };
        }

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

        var links = new PageLinks(baseUrl, page.Value, pageSize, pageCount);
        var _links = new[] { links };

        return new PageResult<TEntity>
        (
            type, elementType,
            page.Value, pageSize,
            error: false, reason: null,
            pageCount, itemCount, _links
        )
        {
            Collection = items
        };
    }

}