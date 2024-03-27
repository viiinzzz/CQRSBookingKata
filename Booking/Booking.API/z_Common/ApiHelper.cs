namespace BookingKata.API.Helpers;

public static class ApiHelper
{
    private static readonly Regex SpaceRx = new(@"\s+", RegexOptions.Multiline);

    private static readonly int DefaultPageSize = 50;


    public static PageResult<TEntity> Page<TEntity>(this IQueryable<TEntity> query, string baseUrl, int? pageSpec, int? pageSizeSpec) where TEntity : class
    {
        var page = pageSpec;
        if (page is null or < 1) page = 1;
        var page0 = page - 1;
        var pageSize = pageSizeSpec ?? DefaultPageSize;
        var itemCount = query.Count();
        var pageCount = itemCount == 0 ? 0 : 1 + (itemCount / pageSize);
        if (page > pageCount) page = pageCount;

        var items = query
            .Skip((page0 ?? 0) * pageSize)
            .Take(pageSize)
            .ToArray();

        var elementType = typeof(TEntity).Name;
        var type = $"{elementType}Collection";

        var links = new PageLinks(baseUrl, page.Value, pageSize, pageCount);

        return new PageResult<TEntity>(
            page.Value, pageSize, pageCount, itemCount, 
            new []{ links }, 
            type, elementType) { Collection = items };
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
