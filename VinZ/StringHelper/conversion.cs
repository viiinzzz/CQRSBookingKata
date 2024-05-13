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

namespace VinZ.Common;

public static partial class StringHelper
{
    public static long ToLong(this string str) => str == null ? 0 : Convert.ToInt64(str);
    public static int ToInt(this string str) => str == null ? 0 : Convert.ToInt32(str);
    public static short ToShort(this string str) => str == null ? (short)0 : Convert.ToInt16(str);
    public static double ToDouble(this string str) => str == null ? 0 : Convert.ToDouble(str);
    public static float ToFloat(this string str) => str == null ? 0 : Convert.ToSingle(str);
}