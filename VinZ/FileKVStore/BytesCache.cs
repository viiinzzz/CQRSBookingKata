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

using VinZ.Common;

namespace VinZ.Common.KVStore;


public record KVStoreConfig(
    string? baseDir, 
    long? diskSizeMax, 
    long? memorySizeMax,
    ILogs.Level? logLevel,
    bool? debugStore
    );


public abstract class BytesCache
{
    private readonly bool _debugStore;
    private readonly IKVStoreBase _store;

    public Logs? Logs => _store.DebugLogs;

    public int DefaultExpireHours { get; init; }


    public BytesCache(KVStoreConfig config)
    {
        _debugStore = config.debugStore ?? throw new ArgumentNullException(nameof(config.debugStore));

        _store = ProvideStoreBase(config);
        //_store = new(baseDir, diskSizeMax, memorySizeMax, expireHours, debugStore) 
        //{
        //    DebugLogs = new Logs(logLevel)
        //};
    }

    protected abstract AbstractKVStoreBase ProvideStoreBase(KVStoreConfig config);

    public async Task Write(string key, byte[] value, int? lifetimeHours, CancellationToken? cancellationToken)
    {
        var hash = key.GetHashCode64();

        var fresh = DateTime.UtcNow;
        var expire = fresh.AddHours(lifetimeHours ?? DefaultExpireHours);

        await _store.Write(hash, value, fresh, expire, key, cancellationToken);
    }

    public async Task<(byte[]? responseBytes, string filePath)> Read(string key, CancellationToken? cancellationToken)
    {
        var hash = key.GetHashCode64();

        var (bytes, status, filePath) = await _store.Read(hash,key, cancellationToken);

        var success = status != ReadStatus.Miss
                     && status != ReadStatus.Expired;

        if (_debugStore && Logs != null)
        {
            var message = @$"
Cache:
  status: {status}
  bytes: {bytes?.Length ?? 0}
  {key.Replace("?", "?\n    ").Replace("&", "\n    ")}";

            if (success)
            {
                Logs?.Always(message);
            }
            else
            {
                Logs?.Warn(message);
            }
        }

        if (success)
        {
            return (bytes, filePath);
        }

        return (null, filePath);
    }


}