/*
 * KVStore
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

namespace VinZ.Common.KVStore.Sqlite;

public partial class KVStoreBaseSqlite
{
    private async Task<bool> CRUD_Check(long key, string description, CancellationToken? cancellationToken)
    {
        if (key == default)
        {
            throw new ArgumentNullException(nameof(key));
        }

        using var connection = Connect();

        var sql = $@"
SELECT Key, Description FROM '{TableName}'
WHERE Key = @key
";
        var res = await connection.QuerySingleOrDefaultAsync(sql, new { key }, cancellationToken);

        ILogEntry? entry = res == null ? null : I.ActLike<ILogEntry>(res);

        CheckCollision(key, description, entry);

        return entry != default;
    }

    private static void CheckCollision(long key, string description, ILogEntry entry)
    {
        if (entry != default && 
            entry.Key == key &&
            entry.Description != description)
        {
            throw new DuplicateNameException(
                @$"Detected Key {key} Description collision:
 - {description}
 - {entry.Description}");
        }
    }

    private async Task CRUD_Check(ILogEntry entry, CancellationToken? cancellationToken)
    {
        await CRUD_Check(entry.Key, entry.Description, cancellationToken);
    }
}