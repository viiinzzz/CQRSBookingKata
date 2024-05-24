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
    public override async Task Write(long key, byte[] bytes, DateTime fresh, DateTime expire, string description,
        CancellationToken? cancellationToken)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        //var transaction = BeginTransaction();

        var exist = await CRUD_Check(key, description, cancellationToken);

        var entry = new LogEntry(key, description, $"{fresh:o}", $"{expire:o}", 1, bytes.Length, null, bytes);

        if (exist)
        {
            await CRUD_Update(entry, false,cancellationToken);

            return;
        }

        await CRUD_Create(entry, cancellationToken);

        //transaction.CommitAsync(cancellationToken ?? CancellationToken.None);
    }
}