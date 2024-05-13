/*
   MIT License
   
   Copyright (c) 2020 Patrik Svensson, Phil Scott
   
   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Drawing;

namespace ANSIConsole;

public static class ANSI
{
    // Source: https://github.com/spectreconsole/spectre.console/blob/3c5b98123b4c9cd50a37da25ca9a3fd34ac7f479/src/Spectre.Console/Internal/Backends/Ansi/AnsiSequences.cs#L32
    /// <summary>
    /// The ASCII escape character (decimal 27).
    /// </summary>
    public const string ESC = "\u001b";

    /// <summary>
    /// Introduces a control sequence that uses 8-bit characters.
    /// </summary>
    public const string CSI = ESC + "[";
    public static string SGR(params byte[] codes) => $"{CSI}{string.Join(";", codes.Select(c => c.ToString()))}m";

    public static string Clear = SGR(0);
    public static string Bold = SGR(1);
    public static string Faint = SGR(2);
    public static string Italic = SGR(3);
    public static string Underlined = SGR(4);
    public static string Blink = SGR(5);
    public static string Inverted = SGR(7);
    public static string StrikeThrough = SGR(9);
    public static string Overlined = SGR(53);

    public static string Foreground(Color color) => SGR(38, 2, color.R, color.G, color.B);
    public static string Background(Color color) => SGR(48, 2, color.R, color.G, color.B);
    public static string Hyperlink(string text, string link) => $"\u001b]8;;{link}\a{text}\u001b]8;;\a";
}