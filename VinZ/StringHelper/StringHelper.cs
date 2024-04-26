using System.Text;

namespace VinZ.Common;

public static class StringHelper
{
    private static readonly Regex SpaceRx = new(@"\s+", RegexOptions.Multiline);


    private static bool? EqualsNull(string? source, string? test)
    {
        var nullCount = 0;
        if (source == null) nullCount++;
        if (test == null) nullCount++;

        if (nullCount == 2) return true;
        if (nullCount == 1) return false;

        return null;
    }

    public static bool EqualsIgnoreCase(this string? source, string? test)
    {
        var ret = EqualsNull(source, test);
        if (ret.HasValue) return ret.Value;

        return 0 == string.Compare(
            source.Trim(), test.Trim(),
            CultureInfo.CurrentCulture,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
    }

    public static bool EqualsIgnoreCaseAndAccents(this string? source, string? test)
    {
        var ret = EqualsNull(source, test);
        if (ret.HasValue) return ret.Value;

        return 0 == string.Compare(
            source.Trim(), test.Trim(),
            CultureInfo.CurrentCulture,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols);
    }

    public static bool EqualsApprox(this string? source, string? test)
    {
        var ret = EqualsNull(source, test);
        if (ret.HasValue) return ret.Value;

        return -1 != CultureInfo.InvariantCulture.CompareInfo.IndexOf(
            SpaceRx.Replace(source.Trim(), ""), SpaceRx.Replace(test.Trim(), ""),
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols);
    }


    public static bool lt(this string a, string b) => a.CompareTo(b) < 0;
    public static bool le(this string a, string b) => a.CompareTo(b) <= 0;
    public static bool eq(this string a, string b) => a.CompareTo(b) == 0;
    public static bool ge(this string a, string b) => a.CompareTo(b) >= 0;
    public static bool gt(this string a, string b) => a.CompareTo(b) > 0;



    public static string SplitEvery(this string str, int every, char separator = ' ')
    {
        var ret = new StringBuilder();
        var i = 0;

        foreach(var c in str)
        {
            if (
                i != 0 && 
                i % every == 0
                )
            {
                ret.Append(separator);
            }

            ret.Append(c);

            i++;
        }

        return ret.ToString();
    }

    public static string xby4(this int n)
    {
        return $"{n.ToString("x8").SplitEvery(4, '-')}";
    }
    public static string xby4(this long n)
    {
        return $"{n.ToString("x16").SplitEvery(4, '-')}";
    }

}