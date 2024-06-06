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

using EntityFrameworkCore.Triggered;

namespace VinZ.Common;

public static partial class DbContextHelper
{
    public record ConfigureMyWayOptions(
        bool isDebug,
        string env,
        LogLevel logLevel,
        Type[]? triggerTypes = default
        ,object? effects = default
    )
    {
        public Type[] BeforeSaveTriggerTypes
        {
            get
            {
                if (triggerTypes == null)
                {
                    return [];
                }

                return triggerTypes
                    .Where(triggerType => triggerType.FindGenericInterfaces(typeof(IBeforeSaveTrigger<>)).Any())
                    .ToArray();
            }
        }

        public Type[] AfterSaveTriggerTypes
        {
            get
            {
                if (triggerTypes == null)
                {
                    return [];
                }

                return triggerTypes
                    .Where(triggerType => triggerType.FindGenericInterfaces(typeof(IAfterSaveTrigger<>)).Any())
                    .ToArray();
            }
        }
    }

    public static void ConfigureMyWay<TContext>(this DbContextOptionsBuilder builder, ConfigureMyWayOptions options)
        where TContext : DbContext
    {
        // if (builder.IsConfigured)
        // {
        //     return;
        // }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{options.env}.json")
            .Build();

        var buildConfigurationName = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyConfigurationAttribute>()
            ?.Configuration;

        var tContext = typeof(TContext);
        var contextName = contextRx.Replace(tContext.Name, "$1");

        var providerName = Environment.GetEnvironmentVariable("PROVIDER_NAME") ??
                           configuration["DbProvider"]?.ToLower() ?? "sqlite";

        var connectionString = connectionStringRx.Replace(
            Environment.GetEnvironmentVariable("ConnectionString")
            ?? configuration.GetConnectionString(buildConfigurationName),
            // ?? configuration.GetConnectionString(env),
            contextName);

        // Console.Error.WriteLine($"contextName={contextName} connectionString={connectionString}");

        var dbbuilder = providerName switch
        {
            "sqlite" => builder.UseSqlite(connectionString),
            "npgsql" => builder.UseNpgsql(connectionString),
            _ => throw new Exception($"unknown providerName {providerName}")
        };

        dbbuilder
            .EnableDetailedErrors(options.isDebug)
            .EnableSensitiveDataLogging(options.isDebug);

        dbbuilder.UseTriggers(triggersOptions =>
        {
            foreach (var beforeSaveTriggerType in options.BeforeSaveTriggerTypes)
            {
                triggersOptions.AddTrigger(beforeSaveTriggerType);
            }

            foreach (var afterSaveTriggerType in options.AfterSaveTriggerTypes)
            {
                triggersOptions.AddTrigger(afterSaveTriggerType);
            }
        });

        if (options.logLevel == LogLevel.Trace)
        {
            builder.LogTo(Console.WriteLine);
        }
    }
}