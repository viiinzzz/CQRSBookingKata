/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.Common;

public static class DbContextHelper
{
    public static void EnsureDatabaseCreated
    (
        this WebApplication app, 
        Type[] contextTypes,
        LogLevel logLevel,
        int? keepAliveMilliseconds = default
    )
    {
        var ensureDatabaseCreated = typeof(DbContextHelper)
            .GetMethod(nameof(EnsureDatabaseCreatedPrivate), 
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException(
            nameof(DbContextHelper), nameof(EnsureDatabaseCreatedPrivate));
        if (logLevel <= LogLevel.Debug)
        {
            Console.Write(@"Storage:
");
        }

        foreach (var contextType in contextTypes)
        {
            if (!contextType.IsAssignableTo(typeof(DbContext)))
            {
                throw new ArgumentException("types must be DbContext", nameof(contextTypes));
            }

            ensureDatabaseCreated
                .MakeGenericMethod([contextType])
                .Invoke(null, [app, logLevel, keepAliveMilliseconds]);
        }
    }

    public static void EnsureDatabaseCreated<TContext>
    (
        this WebApplication app,
        LogLevel logLevel,
        int? keepAliveMilliseconds = default
    )
        where TContext : DbContext
    {
        if (logLevel <= LogLevel.Debug)
        {
            Console.Write(@"Storage:
");
        }

        EnsureDatabaseCreatedPrivate<TContext>(app, logLevel, keepAliveMilliseconds);
    }


    private static void EnsureDatabaseCreatedPrivate<TContext>
    (
        this WebApplication app,
        LogLevel logLevel,
        int? keepAliveMilliseconds = default
    ) 
        where TContext : DbContext
    {
        var database = app
            .Services
            .GetRequiredService<IDbContextFactory>()
            .CreateDbContext<TContext>()
            .Database;

        var created = database.EnsureCreated();

        if (logLevel <= LogLevel.Debug)
        {
            Console.WriteLine(
                @$"{database.GetConnectionString()} {(created ? "created" : "already exists")} for {typeof(TContext).Name}");
        }

        if (keepAliveMilliseconds.HasValue)
        {
            //for in-memory sqlite, as currently configured for the 'Debug' build configuration,
            //at least one client connection needs to be kept open, otherwise the database vanishes
            var dbContext = Activator.CreateInstance(typeof(TContext)) as TContext;

            var keepAlive = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(keepAliveMilliseconds.Value);

                    dbContext.Database.OpenConnection();

                    //                 Console.WriteLine(@$"
                    // {typeof(TContext).Name}: Keep Alive. {database.GetConnectionString()}
                    // ");
                }
            });
        }
    }


    public static void RegisterDbContexts
    (
        this WebApplicationBuilder webApplicationBuilder,
        Type[] myDbContextTypes,
        bool isDebug, 
        LogLevel logLevel
    )
    {
        var registerDbContext = typeof(DbContextHelper)
            .GetMethod(nameof(RegisterDbContextPrivate),
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException(
            nameof(DbContextHelper), nameof(RegisterDbContextPrivate));

        var dbContextFactory = new RegisteredDbContextFactory();

        foreach (var myDbContextType in myDbContextTypes)
        {
            if (!myDbContextType.IsAssignableTo(typeof(MyDbContext)))
            {
                throw new ArgumentException("types must be MyDbContext", nameof(myDbContextTypes));
            }

            registerDbContext
                .MakeGenericMethod([myDbContextType])
                .Invoke(null, [dbContextFactory, isDebug, logLevel]);
        }

        webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
    }


    public static void RegisterDbContext<TContext>
    (
        this RegisteredDbContextFactory dbContextFactory,
        bool isDebug,
        LogLevel logLevel
    )
        where TContext : MyDbContext, new()
    {
        dbContextFactory.RegisterDbContextPrivate<TContext>(isDebug, logLevel);
    }

    private static void RegisterDbContextPrivate<TContext>
    (
        this RegisteredDbContextFactory dbContextFactory,
        bool isDebug,
        LogLevel logLevel
    )
        where TContext : MyDbContext, new()
    {
        dbContextFactory.RegisterDbContextType(() =>
        {
            var dbContext = new TContext
            {
                IsDebug = isDebug,
                logLevel = logLevel
            };

            return dbContext;
        });
    }


    private static readonly Regex contextRx = new("^(.*)Context$", RegexOptions.IgnoreCase);
    private static readonly Regex connectionStringRx = new(@"\(\$Context\)", RegexOptions.IgnoreCase);

    public static void ConfigureMyWay<TContext>(this DbContextOptionsBuilder builder, bool isDebug, LogLevel logLevel)
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

        builder.UseSqlite(connectionString)
            .EnableDetailedErrors(isDebug)
            .EnableSensitiveDataLogging(isDebug);

        if (logLevel == LogLevel.Trace)
        {
            builder.LogTo(Console.WriteLine);
        }
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