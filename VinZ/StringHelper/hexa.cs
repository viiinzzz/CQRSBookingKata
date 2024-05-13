namespace VinZ.Common;

public static partial class StringHelper
{
    public static string SplitEvery(this string str, int every, char separator = ' ')
    {
        var ret = new StringBuilder();
        var i = 0;

        foreach (var c in str)
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