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

using System.Net.NetworkInformation;

namespace VinZ.Common;


public static class DbContextHelper
{
//     public static void WaitNoTransaction(this DbContext dbContext, int millisecondsTimeout = 30_000, int millisecondsWait = 100)
//     {
//         SpinWait.SpinUntil(() => 
//             { 
//                 Thread.Sleep(millisecondsWait);
//
//                 return dbContext.Database.CurrentTransaction == null;
//             },
//             millisecondsTimeout
//         );
//     }

    public static void EnsureDatabaseCreated
    (
        this WebApplication app, 
        IEnumerable<Type> contextTypes,
        LogLevel logLevel,
        int? keepAliveMilliseconds = default
    )
    {
        var ensureDatabaseCreated = typeof(DbContextHelper)
            .GetMethod(nameof(EnsureDatabaseCreatedPrivate), 
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException(
            nameof(DbContextHelper), nameof(EnsureDatabaseCreatedPrivate));

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

        var providerName = database.ProviderName;
        var connectionString = database.GetConnectionString();

        var contextName = contextRx.Replace(typeof(TContext).Name, "$1");

        // Console.Error.WriteLine($"ensurecreated contextName={contextName} connectionString={connectionString}");

        bool created = false;
        try
        {
            created = database.EnsureCreated();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(
                @$"Storage: {database.GetConnectionString()} failure for {typeof(TContext).Name}
Error: {ex.Message}
");
            throw new NetworkInformationException();
        }

        if (logLevel <= LogLevel.Debug)
        {
            app.Logger.LogInformation(
                @$"Storage: {database.GetConnectionString()} {(created ? "created" : "already exists")} for {typeof(TContext).Name}");
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
        IEnumerable<Type> myDbContextTypes,
        bool isDebug, 
        LogLevel logLevel
    )
    {
        var registerDbContextType = typeof(DbContextHelper)
            .GetMethod(nameof(RegisterDbContextTypePrivate),
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException(
            nameof(DbContextHelper), nameof(RegisterDbContextTypePrivate));

        var dbContextFactory = new RegisteredDbContextFactory();

        foreach (var myDbContextType in myDbContextTypes)
        {
            if (!myDbContextType.IsAssignableTo(typeof(MyDbContext)))
            {
                throw new ArgumentException("types must be MyDbContext", nameof(myDbContextTypes));
            }

            registerDbContextType
                .MakeGenericMethod([myDbContextType])
                .Invoke(null, [dbContextFactory, isDebug, logLevel]);
        }

        webApplicationBuilder.Services.AddSingleton<IDbContextFactory>(dbContextFactory);
    }


    public static void RegisterDbContextType<TContext>
    (
        this RegisteredDbContextFactory dbContextFactory,
        bool isDebug,
        LogLevel logLevel
    )
        where TContext : MyDbContext, new()
    {
        dbContextFactory.RegisterDbContextTypePrivate<TContext>(isDebug, logLevel);
    }


    private static void RegisterDbContextTypePrivate<TContext>
    (
        this RegisteredDbContextFactory dbContextFactory,
        bool isDebug,
        LogLevel logLevel
    )
        where TContext : MyDbContext, new()
    {
        dbContextFactory.RegisterDbContextType(() => new TContext
        {
            IsDebug = isDebug,
            logLevel = logLevel
        });
    }


    private static readonly Regex contextRx = new("^(.*)Context$", RegexOptions.IgnoreCase);
    private static readonly Regex connectionStringRx = new(@"\(\$Context\)", RegexOptions.IgnoreCase);

    public static void ConfigureMyWay<TContext>(this DbContextOptionsBuilder builder, bool isDebug, LogLevel logLevel)
        where TContext : DbContext
    {
        // if (builder.IsConfigured)
        // {
        //     return;
        // }

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

        var providerName = Environment.GetEnvironmentVariable("PROVIDER_NAME")  ?? configuration["DbProvider"]?.ToLower() ?? "sqlite";

        var connectionString = connectionStringRx.Replace(
            Environment.GetEnvironmentVariable("ConnectionString") ?? configuration.GetConnectionString(buildConfigurationName),
            contextName);

        // Console.Error.WriteLine($"contextName={contextName} connectionString={connectionString}");

        var dbbuilder = providerName switch
        {
            "sqlite" => builder.UseSqlite(connectionString),
            "npgsql" => builder.UseNpgsql(connectionString),
            _ => throw new Exception($"unknown providerName {providerName}")
        };

        dbbuilder
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