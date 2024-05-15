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

public static class RandomHelper
{
    private static readonly System.Random random = new();

    public static int Rand(this int max) => random.Next(max);
    public static long Rand(this long max) => random.NextInt64(max);
    public static double Rand(this double max) => random.NextDouble() * max;

    public static int Int() => random.Next();
    public static long Long() => random.NextInt64();
    public static (long, long) Guid()
    {
        var guid = System.Guid.NewGuid();

        return guid.ToLong2();
    }

    public static string ToGuidB(this (long, long) twoLongs)
    {
        return new[] { twoLongs.Item1, twoLongs.Item2 }.ToGuid().ToString("B");

    }
    public static Guid ToGuid(this long[] twoLongs)
    {
        var l = twoLongs;

        return new Guid(
            (int)l[0], (short)(l[0] >> 32), (short)(l[0] >> 48),
            (byte)l[1], (byte)(l[1] >> 8), (byte)(l[1] >> 16), (byte)(l[1] >> 24),
            (byte)(l[1] >> 32), (byte)(l[1] >> 40), (byte)(l[1] >> 48), (byte)(l[1] >> 56)
        );
    }

    public static (long, long) ToLong2(this Guid guid)
    {
        var bytes = guid.ToByteArray();

        long l0 = 0;
        long l1 = 0;

        for (var i = 0; i < 8; i++)
        {
            l0 |= (long)bytes[i] << (8 * i);
        }

        for (var i = 8; i < 16; i++)
        {
            l1 |= (long)bytes[i] << (8 * i);
        }

        return (l0, l1);
    }

}