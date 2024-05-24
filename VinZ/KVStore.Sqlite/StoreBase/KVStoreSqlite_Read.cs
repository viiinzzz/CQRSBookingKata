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
    public override async Task<ILogEntry?> HasKey(long key, string description, CancellationToken? cancellationToken)
    {
        return await CRUD_Read(key, description, cancellationToken);
    }

    public override async Task<ReadResult> Read(long key, string description,
        CancellationToken? cancellationToken)
    {
        var entry = await CRUD_Read(key, description, cancellationToken);

        if (entry == default)
        {
            return new ReadResult(default, ReadStatus.Miss, default);
        }

        var pseudoFilePath = this.GetPseudoFilePath(entry);

        var fresh = ILogEntry.GetFresh(entry);
        var expire = ILogEntry.GetExpire(entry);
        var now = DateTime.UtcNow;

        if (now > expire)
        {
            await Delete(key, description, cancellationToken);

            return new ReadResult(default, ReadStatus.Expired, pseudoFilePath);
        }

        await CRUD_Update(ILogEntry.Hit(entry), false, cancellationToken);

        return new ReadResult(entry.FileBytes, ReadStatus.Disk, pseudoFilePath);
    }
}