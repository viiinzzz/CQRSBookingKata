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

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace VinZ.Common;

public static partial class StringHelper
{
    private static BlockingCollection<SHA256> Shas = NewShaCollection();

    private static BlockingCollection<SHA256> NewShaCollection()
    {
        var ret = new BlockingCollection<SHA256>(Environment.ProcessorCount);
        for (var i = 0; i < ret.BoundedCapacity; i++)
        {
            ret.Add(SHA256.Create());
        }

        return ret;
    }

    private static T? GetASha<T>(Func<SHA256, T?> use)
    {
        var sha = Shas.Take();

        Exception? ex = default;
        T ret = default;
        try
        {
            ret = use(sha);
        }
        catch (Exception ex_)
        {
            ex = ex_;
        }

        Shas.Add(sha);

        if (ex != null)
        {
            throw ex;
        }

        return ret;
    }

    public static long GetHashCode64(this string str)
    {
        var bytes = Encoding.ASCII.GetBytes(str);

        return GetHashCode64(bytes);
    }

    public static long GetHashCode64(this byte[] bytes) => GetASha(aSha =>
    {
        var sha = aSha.ComputeHash(bytes); //32 bytes
        ulong uhash = 0;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var b = sha[i * 8 + j];

                uhash ^= (ulong)b << j * 8;
            }
        }

        var hash = unchecked((long)uhash + long.MinValue);

        return hash;
    });
}