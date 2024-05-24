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
    private async Task CRUD_Init(CancellationToken? cancellationToken)
    {
        var databaseFolder = Path.GetDirectoryName(DatabasePath);

        if (!Directory.Exists(databaseFolder))
        {
            Directory.CreateDirectory(databaseFolder);
        }

        if (!File.Exists(DatabasePath))
        {
            SQLiteConnection.CreateFile(DatabasePath);

            using var connection0 = Connect();
            connection0.Open();
        }

        if (!File.Exists(DatabasePath))
        {
            throw new FileNotFoundException(DatabasePath);
        }

        using var connection = Connect();
        
        var sql = $@"
CREATE TABLE IF NOT EXISTS '{TableName}'(
    Key BIGINT NOT NULL PRIMARY KEY,
    Description TEXT,
    Fresh TEXT,
    Expire TEXT,
    HitCount INT,
    FileSize INT,
    FileName TEXT,
    FileBytes BLOB
);
";
        await connection.ExecuteAsync(sql, null, null, null, null, cancellationToken);
    }
}