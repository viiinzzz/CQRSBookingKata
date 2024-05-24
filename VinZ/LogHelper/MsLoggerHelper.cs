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

public static class MsLoggerHelper
{
    public static LogLevel ToMsLogLevel(this Level level)
    {
        return level switch
        {
            Level.Trace => LogLevel.Trace,
            Level.Debug => LogLevel.Debug,
            Level.Info => LogLevel.Information,
            Level.Warn => LogLevel.Warning,
            Level.Error => LogLevel.Error,
            Level.Critical => LogLevel.Critical,
            Level.None => LogLevel.None,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    } 
    public static Level ToILogsLevel(this LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Level.Trace,
            LogLevel.Debug => Level.Debug,
            LogLevel.Information => Level.Info,
            LogLevel.Warning => Level.Warn,
            LogLevel.Error => Level.Error,
            LogLevel.Critical => Level.Critical,
            LogLevel.None => Level.None,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}