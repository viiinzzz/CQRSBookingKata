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

public record PageResult<TEntity> 
(
    int page,
    int pageSize,

    bool error,
    string? reason,

    int pageCount,
    int itemCount,

    PageLinks[] links
)
    : IHaveCollection<TEntity>
    
    where TEntity : class
{
    private static string Get_type()
    {
        if (typeof(TEntity).IsInterface)
        {
            throw new ArgumentException($"Invalid generic type {typeof(TEntity).Name}, must not be an interface.", nameof(TEntity));
        }

        return typeof(PageResult<TEntity>).FullName;
    }

    public string? _type { get; } = Get_type();
    public string? _itemType { get; } = typeof(TEntity).Name;

    public TEntity[]? items { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public IEnumerable<TEntity> Collection
    {
        get => this.GetCollection();
        set => this.SetCollection(value);
    }
}