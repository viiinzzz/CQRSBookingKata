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

public static class EnumerableHelper
{
    public static IEnumerable<int> RangeTo(this int from, int to)
    {
        return Enumerable.Range(from, to - from + 1);
    }

    private static System.Random Random = new System.Random();

    public static IEnumerable<T> AsRandomEnumerable<T>(this IQueryable<T> collection)
        => AsRandomEnumerable(collection.AsEnumerable());

    public static IEnumerable<T> AsRandomEnumerable<T>(this IEnumerable<T> collection)
    {
        var scrambled = collection
            .OrderBy(item => Random.Next());

        foreach (var item in scrambled)
        {
            yield return item;
        }
    }
}