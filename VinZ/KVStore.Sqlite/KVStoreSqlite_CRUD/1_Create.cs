﻿/*
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
    private async Task CRUD_Create(ILogEntry entry, CancellationToken? cancellationToken)
    {
        await CRUD_Check(entry, cancellationToken);

        using var connection = Connect();

        var sql = $@"
INSERT INTO '{TableName}'(Key, Description, Fresh, Expire, HitCount, FileSize, FileName, FileBytes)    
VALUES(@Key, @Description, @Fresh, @Expire, @HitCount, @FileSize, @FileName, @FileBytes)
";
        await connection.ExecuteAsync(sql, entry, null, null, null, cancellationToken);
    }
}