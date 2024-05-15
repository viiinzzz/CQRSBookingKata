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

public static class Types
{
    public static Type[] From<T1>() => new Type[] { typeof(T1) };
    public static Type[] From<T1, T2>() => new Type[] { typeof(T1), typeof(T2) };
    public static Type[] From<T1, T2, T3>() => new Type[] { typeof(T1), typeof(T2), typeof(T3) };
    public static Type[] From<T1, T2, T3, T4>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
    public static Type[] From<T1, T2, T3, T4, T5>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
    public static Type[] From<T1, T2, T3, T4, T5, T6>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
    public static Type[] From<T1, T2, T3, T4, T5, T6, T7>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
    public static Type[] From<T1, T2, T3, T4, T5, T6, T7, T8>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
    public static Type[] From<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
}