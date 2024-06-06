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
using EfSchemaCompare;

namespace VinZ.Common;

public static partial class DbContextHelper
{
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
        using var context = app
            .Services
            .GetRequiredService<IDbContextFactory>()
            .CreateDbContext<TContext>();

        var database = context.Database;

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
                @$"
Database context: {typeof(TContext).Name}
Connection string: {database.GetConnectionString()}
Error: {ex.Message}
");
            throw new NetworkInformationException();
        }


        //seems not to work correctly with records and EFCore
        // if (!created) 
        try
        {
            var comparer = new CompareEfSql();

            var hasErrors = comparer.CompareEfWithDb(context);

            if (hasErrors)
            {
                app.Logger.LogWarning(
                    @$"
Database context: {typeof(TContext).Name}
Connection string: {database.GetConnectionString()} already exists.

However schema seems not to be compatible:
{comparer.GetAllErrors}");

                // throw new DatabaseSchemaIncompatibleException(comparer.GetAllErrors);
            }
        }
        catch (InvalidOperationException ex)
        {
            app.Logger.LogWarning(
                @$"
Database context: {typeof(TContext).Name}
Connection string: {database.GetConnectionString()} already exists for {typeof(TContext).Name}

However schema could not be verified because:
{ex.Message}");
        }


        if (logLevel <= LogLevel.Debug)
        {
            app.Logger.LogInformation(
                @$"
Database context: {typeof(TContext).Name}
Connection string: {database.GetConnectionString()} {(created ? "was created" : "already exists")}.");
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
                    // Database context: {typeof(TContext).Name}
                    // Storage: {database.GetConnectionString()}
                    // Keep Alive!
                    // ");
                }
            });
        }
    }
}