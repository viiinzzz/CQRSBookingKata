namespace VinZ.Common;

public class ParsableNullableDouble : IParsable<ParsableNullableDouble>
{
    public double? Value { get; init; } = null;

    public static ParsableNullableDouble Parse(string value, IFormatProvider? provider)
    {

        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out ParsableNullableDouble nullableDouble)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            nullableDouble = new ParsableNullableDouble();

            return true;
        }

        if (double.TryParse(value, out var doubleValue))
        {
            nullableDouble = new ParsableNullableDouble() { Value = doubleValue };

            return true;
        }

        nullableDouble = new ParsableNullableDouble();

        return false;
    }
}