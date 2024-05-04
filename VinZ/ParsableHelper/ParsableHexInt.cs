namespace VinZ.Common;

public class ParsableHexInt : IParsable<ParsableHexInt>
{
    public int Value { get; init; } = 0;

    public static ParsableHexInt Parse(string value, IFormatProvider? provider)
    {
        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out ParsableHexInt hexInt)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            hexInt = new ParsableHexInt();

            return true;
        }

        try
        {
            hexInt = new ParsableHexInt()
            {
                Value = int.Parse(value.Replace("-", ""), System.Globalization.NumberStyles.HexNumber)
            };

            return true;
        }
        catch (Exception)
        {
            hexInt = new ParsableHexInt();

            return false;
        }
    }
}