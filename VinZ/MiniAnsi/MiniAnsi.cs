using System.Drawing;

namespace VinZ.Common;

public static class MiniAnsi
{
    private const string ESC = "\u001b";
    private const string CSI = $"{ESC}[";
    private static string SGR(params byte[] codes) => $"{CSI}{string.Join(";", codes.Select(c => c.ToString()))}m";

    public static readonly string Rs = SGR(0, 39, 49);
    public static readonly string Bold = SGR(1);
    public static readonly string Faint = SGR(2);
    public static readonly string Italic = SGR(3);
    public static readonly string Underlined = SGR(4);
    public static readonly string Blink = SGR(5);
    public static readonly string Inverted = SGR(7);
    public static readonly string StrikeThrough = SGR(9);
    public static readonly string Overlined = SGR(53);

    public static string Fg(Color color) => SGR(38, 2, color.R, color.G, color.B);
    public static string Bg(Color color) => SGR(48, 2, color.R, color.G, color.B);
    public static string Href(string link, string? text = null) => $"{ESC}]8;;{link}\a{text ?? link}{ESC}]8;;\a{Rs}"; //hyperlink

    public static readonly string UpAndClearStr = $"{CSI}1A{CSI}2K";
    public static readonly string BoldAndBlinkStr = $"{CSI}1{CSI}5";
}
