/*
 * LoggingHelper
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

using Microsoft.Extensions.Logging;

namespace VinZ.Common.Logging;

public class MsLoggerAdapter<T> : ILogger<T>
{
    private readonly ILogs _logs;

    public MsLoggerAdapter(ILogs logs)
    {
        _logs = logs;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                _logs.Trace($"{state} ({eventId})");
                break;

            case LogLevel.Debug:
                _logs.Debug($"{state} ({eventId})");
                break;

            case LogLevel.Information:
                _logs.Info($"{state} ({eventId})");
                break;

            case LogLevel.Warning:
                _logs.Warn($"{state} ({eventId})");
                break;

            case LogLevel.Error:
                _logs.Error($"{state} ({eventId})", exception);
                break;

            case LogLevel.Critical:
                _logs.Critical($"{state} ({eventId})", exception);
                break;

            case LogLevel.None:
                _logs.Always($"{state} ({eventId})");
                break;

            default:
                break;
        }

    }

    public bool IsEnabled(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                return _logs.IsEnabled(ILogs.Level.Trace);

            case LogLevel.Debug:
                return _logs.IsEnabled(ILogs.Level.Debug);

            case LogLevel.Information:
                return _logs.IsEnabled(ILogs.Level.Info);

            case LogLevel.Warning:
                return _logs.IsEnabled(ILogs.Level.Warn);

            case LogLevel.Error:
                return _logs.IsEnabled(ILogs.Level.Error);

            case LogLevel.Critical:
                return _logs.IsEnabled(ILogs.Level.Critical);

            case LogLevel.None:
                return _logs.IsEnabled(ILogs.Level.None);

            default:
                return false;
        }
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }
}