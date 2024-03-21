using System.Security;

namespace CQRSBookingKata.API.Helpers;

public class PageLinks
{
    public PageLinks(string baseUrl, int page, int pageSize, int pageCount)
    {
        url = $"{baseUrl}?page={page}&pageSize={pageSize}";
        next = page >= pageCount ? null : $"{baseUrl}?page={page + 1}&pageSize={pageSize}";
        prev = page <= 1 ? null : $"{baseUrl}?page={page - 1}&pageSize={pageSize}";
    }

    public string url { get; }
    public string? next { get; }
    public string? prev { get; }
}

public record PageResult<TEntity>(
    int page, int pageSize, int pageCount, int itemCount,
    PageLinks[] links,
    string type, string elementType, TEntity[] items
);

public static class ApiHelper
{
    private static readonly Regex SpaceRx = new Regex(@"\s+", RegexOptions.Multiline);

    private static readonly int DefaultPageSize = 40;


    public static PageResult<TEntity> Page<TEntity>(this IQueryable<TEntity> query, string baseUrl, int? pageSpec, int? pageSizeSpec) where TEntity : class
    {
        var page = pageSpec;
        if (page is null or < 1) page = 1;
        var page0 = page - 1;
        var pageSize = pageSizeSpec ?? DefaultPageSize;
        var itemCount = query.Count();
        var pageCount = itemCount / pageSize;
        if (page > pageCount) page = pageCount;

        var items = query
            .Skip((page0 ?? 0) * pageSize)
            .Take(pageSize)
            .ToArray();

        var elementType = typeof(TEntity).Name;
        var type = $"{elementType}Collection";

        var links = new PageLinks(baseUrl, page.Value, pageSize, pageCount);

        return new PageResult<TEntity>(page.Value, pageSize, pageCount, itemCount, new []{ links }, type, elementType, items);
    }

    public static IResult AsResult<TEntity>(this TEntity? result) where TEntity : class
    {
        return result == default
            ? Results.NotFound()
            : Results.Ok(result);
    }
    public static TEntity Patch<TEntity, TPartEntity>(this TEntity current, TPartEntity patch)
        where TEntity : class
        where TPartEntity : class
    {
        if (current == null)
        {
            throw new ArgumentNullException(nameof(current));
        }

        var retObj = JObject.FromObject(current);
        var patchObj = JObject.FromObject(patch);

        retObj?.Merge(patchObj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        return retObj?.ToObject<TEntity>()

               ?? throw new ArgumentNullException(nameof(retObj));
    }



    private static readonly Regex contextRx = new("^(.*)Context$", RegexOptions.IgnoreCase);
    private static readonly Regex connectionStringRx = new(@"\(\$Context\)", RegexOptions.IgnoreCase);

    public static void ConfigureMyWay<TContext>(this DbContextOptionsBuilder builder)
        where TContext : DbContext
    {
        if (builder.IsConfigured)
        {
            return;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var buildConfigurationName = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyConfigurationAttribute>()
            ?.Configuration;

        var tContext = typeof(TContext);
        var contextName = contextRx.Replace(tContext.Name, "$1");

        var connectionString = connectionStringRx.Replace(
            configuration.GetConnectionString(buildConfigurationName),
            contextName);

        builder.UseSqlite(connectionString);
    }

}


/*

[Aspect(Scope.Global)]
[Injection(typeof(TransactionCall))]
public class TransactionCall : Attribute
{
    private IDbContextTransaction _transaction;

    [Advice(Kind.Before)]
    public void LogEnter([Argument(Source.Name)] string methodName)
    {

        _transaction = _back.Database.BeginTransaction();
        _transaction.BeginTransaction();
        _transaction.Commit();
        _transaction.Rollback();
    }
}

*/