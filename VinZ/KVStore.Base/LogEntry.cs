/*
 * KVStore
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

namespace VinZ.Common.KVStore;

public record LogEntry(
    long Key,
    string? Description,
    string? Fresh, 
    string? Expire, 
    int HitCount,
    int FileSize,
    string? FileName,
    byte[]? FileBytes
    ) : ILogEntry
{
    public static readonly LogEntry Default = new(0, null, null, null, 0, 0, null, null);

    public static LogEntry operator ++(LogEntry entry)
    {
        return entry with
        {
            HitCount = entry.HitCount + 1
        };
    }

}
