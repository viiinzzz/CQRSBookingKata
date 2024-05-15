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

using System.Globalization;
using System.Text.RegularExpressions;
using CaseExtensions;

namespace VinZ.Common;

public static partial class StringHelper
{
    /*
     camelCase      theQuickBrownFox
     kebab-case     the-quick-brown-fox
     PascalCase     TheQuickBrownFox
     snake_case     the_quick_brown_fox
     Train-Case     The-Quick-Brown-Fox
     */
     
    public static string ToCamelCase(this string str) => StringExtensions.ToCamelCase(str);
    public static string ToKebabCase(this string str) => StringExtensions.ToKebabCase(str);
    public static string ToPascalCase(this string str) => StringExtensions.ToPascalCase(str);
    public static string ToSnakeCase(this string str) => StringExtensions.ToSnakeCase(str);
    public static string ToTrainCase(this string str) => StringExtensions.ToTrainCase(str);

    public static string ToTitleCase(this string str) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str);

    private static Regex _CapRx = new Regex(@"([\p{Lu}\p{Lt}])");
    public static string ToSpaceCase(this string str) => _CapRx.Replace(str.ToPascalCase(), " $1");

}