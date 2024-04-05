namespace VinZ.Common;

public class ParsableNullableInt : IParsable<ParsableNullableInt>
{
    public int? Value { get; init; } = null;

    public static ParsableNullableInt Parse(string value, IFormatProvider? provider)
    {

        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out ParsableNullableInt nullableInt)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            nullableInt = new ParsableNullableInt();

            return true;
        }

        if (int.TryParse(value, out var intValue))
        {
            nullableInt = new ParsableNullableInt() { Value = intValue };

            return true;
        }

        nullableInt = new ParsableNullableInt();

        return false;
    }
}