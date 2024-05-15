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
    public static string ByteSize(this long size)
    {
        if (size < 0)
        {
            return "0 byte";
        }

        if (size == 1)
        {
            return "1 byte";
        }

        if (size < 1024L)
        {
            return $"{size} bytes";
        }

        if (size < 1024L * 1024)
        {
            return $"{size / 1024d:0.0}KB";
        }

        if (size < 1024L * 1024 * 1024)
        {
            return $"{size / 1024d / 1024:0.0}MB";
        }

        if (size < 1024L * 1024 * 1024 * 1024)
        {
            return $"{size / 1024d / 1024 / 1024:0.0}MB";
        }

        return $"{size / 1024d / 1024 / 1024 / 1024: 0.0}GB";
    }

    public static string UnitSize(this long size, string unit, bool space, bool pluralize)
    {
        if (size < 0)
        {
            return $"0 {unit}";
        }

        if (size == 1)
        {
            return $"1 {unit}";
        }

        if (size < 1024L)
        {
            return $"{size} {unit}{(pluralize ? "s" : "")}";
        }

        if (size < 1024L * 1024)
        {
            return $"{size / 1024d:0.0}{(space ? " " : "")}K{unit}{(pluralize ? "s" : "")}";
        }

        if (size < 1024L * 1024 * 1024)
        {
            return $"{size / 1024d / 1024:0.0}{(space ? " " : "")}M{unit}{(pluralize ? "s" : "")}";
        }

        if (size < 1024L * 1024 * 1024 * 1024)
        {
            return $"{size / 1024d / 1024 / 1024:0.0}{(space ? " " : "")}M{unit}{(pluralize ? "s" : "")}";
        }

        return $"{size / 1024d / 1024 / 1024 / 1024: 0.0}{(space ? " " : "")}G{unit}{(pluralize ? "s" : "")}";
    }

    public static string UnitSize(this double size, string unit, bool space = false, int digits = 1,
        bool pluralize = true)
    {
        if (size < 0)
        {
            return $"0{unit}";
        }

        if (size == 1)
        {
            return $"1{unit}";
        }

        if (size < 1024d)
        {
            return $"{size.ToString($"F{digits}")}{unit}{(pluralize ? "s" : "")}";
        }

        if (size < 1024d * 1024)
        {
            return $"{(size / 1024d).ToString($"F{digits}")}{(space ? " " : "")}K{unit}{(pluralize ? "s" : "")}";
        }

        if (size < 1024d * 1024 * 1024)
        {
            return $"{(size / 1024d / 1024).ToString($"F{digits}")}{(space ? " " : "")}M{unit}{(pluralize ? "s" : "")}";
        }

        if (size < 1024d * 1024 * 1024 * 1024)
        {
            return
                $"{(size / 1024d / 1024 / 1024).ToString($"F{digits}")}{(space ? " " : "")}M{unit}{(pluralize ? "s" : "")}";
        }

        return
            $"{(size / 1024d / 1024 / 1024 / 1024).ToString($"F{digits}")}{(space ? " " : "")}G{unit}{(pluralize ? "s" : "")}";
    }
}