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

using VinZ.Common.Logging;

namespace VinZ.Common.KVStore;

public interface IKVStoreBase
{
    string BaseDir { get; }
    long DiskSizeMax { get; }
    Logs? DebugLogs { get; }
    Task Write(long key, byte[] bytes, DateTime fresh, DateTime expire, string description, CancellationToken? cancellationToken);
    Task<ReadResult> Read(long key, string description, CancellationToken? cancellationToken);
    Task<bool> Delete(long key, string description, CancellationToken? cancellationToken);
    Task<ILogEntry?> HasKey(long key, string description, CancellationToken? cancellationToken);
}