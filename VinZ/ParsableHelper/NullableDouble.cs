namespace VinZ.ParsableHelper;

public class NullableDouble : IParsable<NullableDouble>
{
    public double? Value { get; init; } = null;

    public static NullableDouble Parse(string value, IFormatProvider? provider)
    {

        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out NullableDouble nullableDouble)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            nullableDouble = new NullableDouble();

            return true;
        }

        if (double.TryParse(value, out var doubleValue))
        {
            nullableDouble = new NullableDouble() { Value = doubleValue };

            return true;
        }

        nullableDouble = new NullableDouble();

        return false;
    }
}