/*
 * StringHelper
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

public static class ConsoleAsync
{
    private static Task<string?>? _readLineTask;

    public static async Task<string?> ReadLine(CancellationToken cancellationToken)
    {
        while (_readLineTask != null || _readKeyTask != null)
        {
            await Task.Delay(500, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
        }

        
        _readLineTask = Task.Run(Console.ReadLine, cancellationToken);

        var cancel = Task.Delay(-1, cancellationToken);
        await Task.WhenAny(_readLineTask, cancel);
        cancellationToken.ThrowIfCancellationRequested();

        var line = await _readLineTask;
        _readLineTask = null;

        if (line == null)
        {
            throw new OperationCanceledException();
        }
        return line;
        
    }

    private static Task<ConsoleKeyInfo>? _readKeyTask;

    public static async Task<ConsoleKeyInfo> ReadKey(CancellationToken cancellationToken)
    {
        while (_readLineTask != null || _readKeyTask != null)
        {
            await Task.Delay(500, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
        }

        
        _readKeyTask = Task.Run(Console.ReadKey, cancellationToken);

        var cancel = Task.Delay(-1, cancellationToken);
        await Task.WhenAny(_readKeyTask, cancel);
        cancellationToken.ThrowIfCancellationRequested();

        var key = await _readKeyTask;
        _readKeyTask = null;

        if (key == null)
        {
            throw new OperationCanceledException();
        }
        return key;
        
    }
}