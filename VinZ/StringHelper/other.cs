/*
 * StringHelper
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

using Pluralize.NET;
using System.Text;
using System.Text.RegularExpressions;

namespace VinZ.Common;

public static partial class StringHelper
{
    public static string Truncate(this string str, int maxLength)
    {
        if (str == null || str.Length < maxLength)
        {
            return str;
        }

        return str.Substring(0, maxLength);
    }

    public static string TruncateNumberList(this string text)
    {
        var rx = new Regex(@"(\d+\.*\d+)( \d+\.*\d+)+", RegexOptions.Multiline);
        return rx.Replace(text, "_");
    }


    public static string IncrementString(this string str)
    {
        var rx = new Regex(@"^(.*)(\d+)$");
        var m = rx.Match(str);

        if (!m.Success)
        {
            return $"{str}2";
        }

        var s = m.Groups[1].Value;
        int.TryParse(m.Groups[2].Value, out var i);

        return $"{s}{i + 1}";
    }


    public static long NextLong(this Random random)
    {
        var bytes = new byte[8];
        random.NextBytes(bytes);

        return BitConverter.ToInt64(bytes, 0);
    }

    public static string NextString(this Random random, int length)
    {
        var chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = (char)random.Next(char.MinValue, char.MaxValue);
        }
        return new string(chars);
    }


    public static string LetterOrDigitOnly(this string str) => 
        new string(str.Where(char.IsLetterOrDigit).ToArray());


    private static Regex multipleUnserscore = new(@"_+");

    private static bool IsLetterDashUnderscore(this char c) 
        => (char.IsAscii(c) && char.IsLetter(c)) || 
           c == '-' ||
           c == '_';

    public static string? LetterDashUnderscore(this string? str)
        => str == null ? null : multipleUnserscore.Replace(new string(str
            .Normalize(NormalizationForm.FormD)
            .Select(c => c.IsLetterDashUnderscore() ? c : '_')
            .ToArray()
        ), "_");


    private static Regex frontDigit = new(@"^[0-9]+");

    private static bool IsLetterUnderscoreDigit(this char c)
        => (char.IsAscii(c) && char.IsLetter(c)) ||
           (char.IsAscii(c) && char.IsNumber(c)) ||
           c == '_';

    public static string? LetterUnderscoreDigit(this string? str)
        => str == null
            ? null
            : multipleUnserscore.Replace(new string(str
                .Normalize(NormalizationForm.FormD)
                .Select(c => c.IsLetterUnderscoreDigit() ? c : '_')
                .ToArray()
            ), "_");


    public static string? LetterUnderscoreNoFrontDigit(this string? str, bool noFrontDigit = true)
        => str == null 
            ? null 
            : LetterUnderscoreDigit(noFrontDigit ? frontDigit.Replace(str, "") : str);



    public static string? ToCodeCase(this string? str, bool noFrontDigit = true)
    {
        return str?.LetterUnderscoreNoFrontDigit(noFrontDigit)?.ToPascalCase();
    }


    private static IPluralize _pluralizer = new Pluralizer();

    public static string Pluralize(this string str) => _pluralizer.Pluralize(str);

}