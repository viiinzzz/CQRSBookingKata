namespace VinZ.ParsableHelper;

public class NullableInt : IParsable<NullableInt>
{
    public int? Value { get; init; } = null;

    public static NullableInt Parse(string value, IFormatProvider? provider)
    {

        if (!TryParse(value, provider, out var result))
        {
            throw new ArgumentException("Could not parse supplied value.", nameof(value));
        }

        return result;
    }

    public static bool TryParse(string? value, IFormatProvider? provider, out NullableInt nullableInt)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            nullableInt = new NullableInt();

            return true;
        }

        if (int.TryParse(value, out var intValue))
        {
            nullableInt = new NullableInt() { Value = intValue };

            return true;
        }

        nullableInt = new NullableInt();

        return false;
    }
}