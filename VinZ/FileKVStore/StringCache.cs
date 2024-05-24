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

using System.Text;

namespace VinZ.Common.KVStore;

public abstract class StringCache
{
    private readonly BytesCache _store;

    public StringCache(KVStoreConfig config)
    {
        _store = ProvideStoreBase(config);
    }

    protected abstract BytesCache ProvideStoreBase(KVStoreConfig config);


    public Logs Logs => _store.Logs;


    public async Task Write(string key, string value, int? lifetimeHours, CancellationToken? cancellationToken)
    {
        await _store.Write(key, Encoding.UTF8.GetBytes(value), lifetimeHours, cancellationToken);
    }

    public async Task<(string? value, string filePath)> Read(string key, CancellationToken? cancellationToken)
    {
        var (bytes, filePath) = await _store.Read(key, cancellationToken);

        var value = bytes == null ? null : Encoding.UTF8.GetString(bytes);

        return (value, filePath);
    }
}