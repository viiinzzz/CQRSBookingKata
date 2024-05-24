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

using System.Globalization;

namespace VinZ.Common.KVStore;

public interface ILogEntry
{
    long Key { get; }
    string? Description { get; }
    string? Fresh { get; }
    string? Expire { get; }
    int HitCount { get;}
    int FileSize { get;  }
    string? FileName { get; }
    byte[]? FileBytes { get;  }


    public static DateTime GetFresh(ILogEntry? entry)
    {
        if (entry == null ||
            !DateTime.TryParse(entry.Fresh, null, DateTimeStyles.RoundtripKind, out var time))
        {
            return DateTime.MinValue;
        }

        return time;
    }

    public static DateTime GetExpire(ILogEntry? entry)
    {
        if (entry == null ||
            !DateTime.TryParse(entry.Expire, null, DateTimeStyles.RoundtripKind, out var time))
        {
            return DateTime.MaxValue;
        }

        return time;
    }

    public static ILogEntry Hit(ILogEntry entry)
    {
        return new LogEntry(entry.Key, entry.Description, entry.Fresh, entry.Expire, entry.HitCount + 1, entry.FileSize, entry.FileName, entry.FileBytes);
    }
}


/*
 maybe rework
https://stackoverflow.com/questions/12510299/get-datetime-as-utc-with-dapper

public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
   {
   public override void SetValue(IDbDataParameter parameter, DateTime value)
   {
   parameter.Value = value;
   }
   
   public override DateTime Parse(object value)
   {
   return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
   }
   }
   
   Then in my Global.asax file of my Web API I add the type handler to dapper.
   
   SqlMapper.AddTypeHandler(new DateTimeHandler());
   
   If you need to ensure you are always inserting dates as UTC, then on the SetValue method you can use:
   
   parameter.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
   
 
 */