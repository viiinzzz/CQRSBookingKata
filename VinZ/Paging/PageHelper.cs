namespace VinZ.Common;

public static class PageHelper
{
    public static PageLinks CreateLinks(string baseUrl, int page, int pageSize, int pageCount)
    {
        return new PageLinks
        (
            url: $"{baseUrl}?page={page}&pageSize={pageSize}",
            next: page >= pageCount ? null : $"{baseUrl}?page={page + 1}&pageSize={pageSize}",
            prev: page <= 1 ? null : $"{baseUrl}?page={page - 1}&pageSize={pageSize}"
        );
    }


    private static readonly int DefaultPageSize = 50;

    public static PageResult<TEntity> Page<TEntity>
    (
        this IQueryable<TEntity> query,
        string baseUrl,
        int? pageSpec,
        int? pageSizeSpec
    )
        where TEntity : class
    {
        var queryCheck = (IQueryable<TEntity> q) =>
        {
            q.LostInTranslation(out var sql, out var translationError, doThrow: false);
        };


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
            var pageResult = new PageResult<TEntity>
            (
                type, elementType,
                page.Value, pageSize,
                error: true, reason: ex.Message,
                0, 0, Array.Empty<PageLinks>()
            );

            pageResult.SetCollection(Enumerable.Empty<TEntity>());

            return pageResult;
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

        var links = CreateLinks(baseUrl, page.Value, pageSize, pageCount);
        var _links = new[] { links };

        {
            var pageResult = new PageResult<TEntity>
            (
                type, elementType,
                page.Value, pageSize,
                error: false, reason: null,
                pageCount, itemCount, _links
            );

            pageResult.SetCollection(items);

            return pageResult;
        }
    }

}
