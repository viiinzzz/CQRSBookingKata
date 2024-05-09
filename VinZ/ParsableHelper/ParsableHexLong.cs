namespace VinZ.Common;

public class ParsableHexLong : IParsable<ParsableHexLong>
{
    public long Value { get; init; } = 0;

    public static ParsableHexLong Parse(string value, IFormatProvider? provider)
    {
        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out ParsableHexLong hexLong)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            hexLong = new ParsableHexLong();

            return true;
        }

        try
        {
            hexLong = new ParsableHexLong()
            {
                Value = long.Parse(value.Replace("-", ""), System.Globalization.NumberStyles.HexNumber)
            };

            return true;
        }
        catch (Exception)
        {
            hexLong = new ParsableHexLong();

            return false;
        }
    }
}