namespace VinZ.Common;

public static class DbContextHelper
{
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




    public static bool LostInTranslation<TEntity>
    (
        this IQueryable<TEntity> query,
        out string? sql,
        out string? translationError,
        bool doThrow = false
    ) 
        where TEntity : class
    {
        try
        {
            sql = query.ToQueryString();
            translationError = null;

            return false;
        }
        catch (Exception ex)
        {
            sql = null;
            translationError = ex.Message;

            if (doThrow)
            {
                throw new Exception($"We apologize, we got lost in translation: {translationError}");
            }

            Console.Error.WriteLine($@"
!!!ERROR!!!
Lost in translation!

{translationError}
");

            return true;
        }
    }

}