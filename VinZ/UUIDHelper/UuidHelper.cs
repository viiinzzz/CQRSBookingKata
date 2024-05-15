/*
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

namespace VinZ.Common;

public static class UuidHelper
{
    private static string? _uuid;
    private static long? _uuidInt64;

    public static string GetUuid()
    {
        if (_uuid != null)
        {
            return _uuid;
        }

        _uuid = new DeviceIdBuilder()
            .AddMachineName()
            .AddOsVersion()
            .OnWindows(windows => windows
                .AddProcessorId()
                .AddMotherboardSerialNumber()
                .AddSystemDriveSerialNumber())
            .OnLinux(linux => linux
                .AddMotherboardSerialNumber()
                .AddSystemDriveSerialNumber())
            .OnMac(mac => mac
                .AddSystemDriveSerialNumber()
                .AddPlatformSerialNumber())
            .ToString();

        if (_uuid == null) _uuid = "";

        return _uuid;
    }

    public static long GetUuidInt64()
    {
        if (_uuidInt64.HasValue)
        {
            return _uuidInt64.Value;
        }

        _uuidInt64 = GetUuid().GetHashCode64();

        return _uuidInt64.Value;
    } 
}
