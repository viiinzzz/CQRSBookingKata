namespace CQRSBookingKata.Common;

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
}