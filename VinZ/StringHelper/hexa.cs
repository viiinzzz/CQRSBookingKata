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

public static partial class StringHelper
{
    public static string SplitEvery(this string str, int every, char separator = ' ')
    {
        var ret = new StringBuilder();
        var i = 0;

        foreach (var c in str)
        {
            if (
                i != 0 &&
                i % every == 0
            )
            {
                ret.Append(separator);
            }

            ret.Append(c);

            i++;
        }

        return ret.ToString();
    }

    public static string xby4(this int n)
    {
        return $"{n.ToString("x8").SplitEvery(4, '-')}";
    }

    public static string xby4(this long n)
    {
        return $"{n.ToString("x16").SplitEvery(4, '-')}";
    }
}