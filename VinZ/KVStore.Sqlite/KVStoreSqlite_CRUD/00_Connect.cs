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
    private IDbConnection Connect()
    {
        return Connect(false, out var _);
    }

    private SQLiteTransaction BeginTransaction()
    {
        Connect(true, out var transaction);

        return transaction;
    }

    private IDbConnection Connect(bool beginTransaction, out SQLiteTransaction transaction)
    {
        try
        {
            var connection = new SQLiteConnection(ConnectionString);

            transaction = null;

            if (!beginTransaction) return connection;

            transaction = connection.BeginTransaction();

            return connection;
        }
        catch (Exception ex)
        {
            var ex0 = ex;
            while (ex.InnerException != null) ex = ex.InnerException;
            throw new Exception($"database creation failure: ConnectionString={ConnectionString}; reason: {ex.Message}",
                ex0);
        }
    }

}