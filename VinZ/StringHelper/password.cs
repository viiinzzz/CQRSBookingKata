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

using System.Runtime.InteropServices;
using System.Security;

namespace VinZ.Common;

public static partial class StringHelper
{
    public static async Task<string> ReadPassword(char mask, CancellationToken? cancellationToken)
    {
        const int enterCode = 13, backspaceCode = 8, controlBackspaceCode = 127;
        int[] filteredCodes = { 0, 27, 9, 10, 32 };

        var securePass = new SecureString();

        char chr;

        var readChar = () =>
        {
            return Console.ReadKey(true).KeyChar;
        };

        while ((chr = readChar()) != enterCode)
        {
            var isBackspace =
                chr == backspaceCode ||
                chr == controlBackspaceCode;

            if (isBackspace && securePass.Length > 0)
            {
                Console.Write("\b \b");
                securePass.RemoveAt(securePass.Length - 1);

                continue;
            }

            if (isBackspace && securePass.Length == 0)
            {
                continue;
            }

            if (filteredCodes.Any(x => chr == x))
            {
                continue;
            }

            securePass.AppendChar(chr);
            Console.Write($"{mask}");
        }

        Console.WriteLine();

        var ptr = new IntPtr();
        ptr = Marshal.SecureStringToBSTR(securePass);
        var plainPass = Marshal.PtrToStringBSTR(ptr);
        Marshal.ZeroFreeBSTR(ptr);

        return plainPass;
    }
}