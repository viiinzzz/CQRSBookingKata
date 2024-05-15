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

public class ScopeProvider(IServiceProvider sp, IConfiguration appConfig, ILogger<ScopeProvider> log) : IScopeProvider
{
    private readonly LogLevel logLevel = GetLogLevel(appConfig);

    private static LogLevel GetLogLevel(IConfiguration appConfig)
    {

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:Default"], true, out var logLevelDefault))
        {
            logLevelDefault = LogLevel.Warning;
        }

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:ScopeProvider"], true, out var logLevel))
        {
            logLevel = logLevelDefault;
        }

        return logLevel;
    }

    public IServiceScope GetScope<T>(out T t) where T : notnull
    {
        var scope = sp.CreateScope();

        t = scope.ServiceProvider.GetRequiredService<T>();

        if (logLevel <= LogLevel.Debug)
        {
            log.LogWarning(@$"

                                              ...GetScope {typeof(T).FullName}...
");
        }

        return scope;
    }

    public IServiceScope GetScope(Type serviceType, out object t)
    {
        var scope = sp.CreateScope();

        t = scope.ServiceProvider.GetRequiredService(serviceType);

        if (logLevel <= LogLevel.Debug)
        {
            log.LogWarning(@$"

                                              ...GetScope {serviceType.FullName}...
");
        }

        return scope;
    }

}