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

namespace VinZ.Common.Logging;

public partial interface ILogs
{
    bool IsEnabled(ILogs.Level level);

    void Trace(string text);
    void Debug(string text);
    void Info(string text);
    void Warn(string text);
    void Error(string text);
    void Error(Exception ex);
    void Error(string message, Exception ex);
    void Critical(string text);
    void Critical(Exception ex);
    void Critical(string message, Exception ex);
    void Always(string text);

    string Print(Level level);
}