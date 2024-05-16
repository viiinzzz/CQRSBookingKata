/*
   MIT License
   
   Copyright (c) 2020 Patrik Svensson, Phil Scott
   
   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Runtime.InteropServices;

namespace ANSIConsole;

public static class ANSIInitializer
{
    private static readonly int STD_OUTPUT_HANDLE = -11;
    private static readonly uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private static readonly uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

#if Windows
    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern uint GetLastError();
#elif Linux
    private static bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode)
    {
        lpMode = ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;

        return true;
    }

    private static bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode)
    {
        return true;
    }
   
    private static IntPtr GetStdHandle(int nStdHandle)
    {
        return 0;
    }
   
    private static uint GetLastError()
    {
        return 0;
    }
#endif

    /// <summary>
    /// Run once before using the console.
    /// You may not need to initialize the ANSI console mode.
    /// </summary>
    /// <returns>true if initialization was successful</returns>
    public static bool Init(bool printError = true)
    {
        var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
        if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
        {
            if (printError) Console.WriteLine("failed to get output console mode");
            return false;
        }

        outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
        if (!SetConsoleMode(iStdOut, outConsoleMode))
        {
            if (printError) Console.WriteLine($"failed to set output console mode, error code: {GetLastError()}");
            return false;
        }

        return true;
    }

    private static bool? _enabled;
    public static bool Enabled
    {
        get
        {
            if (_enabled != null) return (bool)_enabled;
            _enabled = Environment.GetEnvironmentVariable("NO_COLOR") == null;
            return Enabled;
        }
        set => _enabled = value;
    }
}