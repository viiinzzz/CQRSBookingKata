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

using System.Data.SQLite;
using VinZ.Common.Logging;

namespace VinZ.Common.KVStore.Sqlite;

public partial class KVStoreBaseSqlite : AbstractKVStoreBase
{
    public string BaseDir { get; init; }
    public long DiskSizeMax { get; init; }
    public Logs? DebugLogs { get; init; }

    private readonly bool _debugStore;

    public string DatabasePath { get; }
    public string JournalPath { get; }
    public string ConnectionString { get; }
    private string TableName { get; } = "LogEntries";

    public int? PageSize { get; init; } = default;
    /*
     PageSize = 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536 (0xffff)
     
    https://www.sqlite.org/limits.html

    The page_size pragma gets or sets the size of the database pages.
    The bytes size must be a power of two.
    By default, the allowed sizes are
    512, 1024, 2048, 4096, 8192, 16384, and 32768 bytes.
    This value becomes fixed once the database is initialized.
    The only way to alter the page size on an existing database is
    to set the page size and then immediately VACUUM the database.
    The default page size is calculated from a number of factors.
    The default page size starts at 1024 bytes.
    If the datatype driver indicates that the native I/O block of the filesystem is larger,
    that larger value will be used up to the maximum default size,
    which is normally set to 8192.
    These values can be altered with the SQLITE_DEFAULT_PAGE_SIZE, SQLITE_MAX_DEFAULT_PAGE_SIZE,
    and SQLITE_MAX_PAGE_SIZE compiler-time directives.
    The end result is that the page size for file-based databases will typically be
    between 1 KB and 4 KB on Microsoft Windows,
    and 1 KB on most other systems, including Mac OS X, Linux, and other Unix systems.
    */

    public KVStoreBaseSqlite(
        string baseDir,
        long diskSizeMax,
        bool debugStore)
    {
        _debugStore = debugStore;

        BaseDir = baseDir;
        DiskSizeMax = diskSizeMax;

        DatabasePath = $"{baseDir}.sqlite";
        JournalPath = $"{baseDir}.sqlite-journal";

        var connectionStringBuilder = PageSize.HasValue ? new SQLiteConnectionStringBuilder()
        {
            DataSource = DatabasePath,
            JournalMode = SQLiteJournalModeEnum.Truncate,
            Pooling = true,
            PageSize = PageSize.Value
        }: new SQLiteConnectionStringBuilder()
        {
            DataSource = DatabasePath,
            JournalMode = SQLiteJournalModeEnum.Truncate,
            Pooling = true
        };

        ConnectionString = connectionStringBuilder.ToString();

        CancellationToken? initCancel = CancellationToken.None;

        CRUD_Init(initCancel)
            .ContinueWith(prev =>
                CompactLogs(initCancel)
                ).Unwrap()
            .ConfigureAwait(false).GetAwaiter().GetResult();
    }


    ~KVStoreBaseSqlite()
    {
        DisposeJournal();
    }

    private void DisposeJournal()
    {
        try
        {
            if (File.Exists(JournalPath))
            {
                File.Delete(JournalPath);
            }
        }
        catch
        {
            DebugLogs?.Warn($"Could not dispose journal file: {JournalPath}");
        }
    }
}