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

using System.Dynamic;

namespace helpers;

public static class IncludeErrorHelper
{

    public static ExpandoObject? IncludeError(this object obj, Exception ex)
    {
        var ret = obj.PatchRelax(new
        {
            _error = ex.Message,
        });


        if (ex.InnerException != null)
        {
            ret = ret.PatchRelax(new
            {
                _errorInner = (new { }).IncludeError(ex.InnerException)
            });
        }

        if (!ex.Message.StartsWith("SQLite Error"))
        {
            var stacktrace = (ex.StackTrace ?? string.Empty)
                .Split(Environment.NewLine)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0 &&
                               !line.StartsWith("at Microsoft.EntityFrameworkCore"))
                .ToArray();

            if (stacktrace.Length > 0)
            {
                ret = ret.PatchRelax(new
                {
                    _errorStackTrace = stacktrace
                });
            }
        }


        //ret = JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(ret));

        return ret;
    }
}