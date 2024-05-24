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
    public override async Task<bool> Delete(long key, string description, CancellationToken? cancellationToken)
    {
        return await CRUD_Delete(key, description, cancellationToken);
    }

    public async Task CompactLogs(CancellationToken? cancellationToken)
    {
        try
        {
            var entries = await List(cancellationToken);
           

            cancellationToken?.ThrowIfCancellationRequested();

            var entries2 = entries
                .GroupBy(entry => entry.Key)
                .Select(keyGroup => keyGroup.MaxBy(entry => entry.HitCount));

            var totalFileSize = entries2.Sum(entry => entry.FileSize);

            if (totalFileSize < DiskSizeMax)
            {
                return;
            }

            var keyGroups = entries2.GroupBy(entry => entry.Key).Select(g =>
            {
                var mostRecent = g.MaxBy(entry => entry.Fresh);
                return new
                {
                    g.Key,
                    mostRecent,
                    markDelete = g.Where(entry => entry != mostRecent)
                };
            });

            var markDelete = keyGroups
                .SelectMany(keyGroup => keyGroup.markDelete);

            var notDeleted = new List<ILogEntry>();

            foreach (var entry in markDelete)
            {
                var delete = await Delete(entry.Key, null, cancellationToken);

                if (!delete)
                {
                    notDeleted.Add(entry);
                }
            }

            var notDeletedFileSize = notDeleted.Sum(entry => entry.FileSize);

            var mostRecents = keyGroups
                .Select(keyGroup => keyGroup.mostRecent)
                .OrderBy(entry => entry.Fresh)
                .ToList();

            var mostRecentsFileSize = mostRecents.Sum(entry => entry.FileSize);

            while (mostRecentsFileSize > (DiskSizeMax - notDeletedFileSize)
                   && mostRecents.Count > 0)
            {
                var entry = mostRecents.First();
                mostRecents.Remove(entry);

                var delete = await Delete(entry.Key, null, cancellationToken);

                if (!delete)
                {
                    notDeleted.Add(entry);
                }
            }

            var compacted = notDeleted
                .Concat(mostRecents)
                .OrderBy(ILogEntry.GetFresh);


            foreach (var entry in entries)
            {
                if (!compacted.Contains(entry))
                {
                    await Delete(entry.Key, null, cancellationToken);

                    cancellationToken?.ThrowIfCancellationRequested();
                }
            }

        }
        catch (Exception ex)
        {
            ;
        }
    }
}