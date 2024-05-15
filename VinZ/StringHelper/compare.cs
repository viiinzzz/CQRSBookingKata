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
    private static readonly Regex SpaceRx = new(@"\s+", RegexOptions.Multiline);


    private static bool? EqualsNull(string? source, string? test)
    {
        var nullCount = 0;
        if (source == null) nullCount++;
        if (test == null) nullCount++;

        if (nullCount == 2) return true;
        if (nullCount == 1) return false;

        return null;
    }

    public static bool EqualsIgnoreCase(this string? source, string? test)
    {
        var ret = EqualsNull(source, test);
        if (ret.HasValue) return ret.Value;

        return 0 == string.Compare(
            source.Trim(), test.Trim(),
            CultureInfo.CurrentCulture,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
    }

    public static bool EqualsIgnoreCaseAndAccents(this string? source, string? test)
    {
        var ret = EqualsNull(source, test);
        if (ret.HasValue) return ret.Value;

        return 0 == string.Compare(
            source.Trim(), test.Trim(),
            CultureInfo.CurrentCulture,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols);
    }

    public static bool EqualsApprox(this string? source, string? test)
    {
        var ret = EqualsNull(source, test);
        if (ret.HasValue) return ret.Value;

        return -1 != CultureInfo.InvariantCulture.CompareInfo.IndexOf(
            SpaceRx.Replace(source.Trim(), ""), SpaceRx.Replace(test.Trim(), ""),
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols);
    }


    public static bool lt(this string a, string b) => a.CompareTo(b) < 0;
    public static bool le(this string a, string b) => a.CompareTo(b) <= 0;
    public static bool eq(this string a, string b) => a.CompareTo(b) == 0;
    public static bool ge(this string a, string b) => a.CompareTo(b) >= 0;
    public static bool gt(this string a, string b) => a.CompareTo(b) > 0;
}