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

public static class PageResultHelper
{
    public static IEnumerable<TEntity> GetCollection<TEntity>(this PageResult<TEntity> pageResult)
        where TEntity : class
    {
        return pageResult.items?.AsEnumerable()
               ?? Enumerable.Empty<TEntity>();
    }

    public static void SetCollection<TEntity>(this PageResult<TEntity> pageResult, IEnumerable<TEntity> items)
        where TEntity : class
    {
        var previousItemsLength = pageResult.items?.Length;

        pageResult.items = items.ToArray();

        if (previousItemsLength.HasValue &&
            pageResult.items.Length != previousItemsLength)
        {
            throw new ArgumentException("must be same Count() as previously", nameof(items));
        }
    }
}