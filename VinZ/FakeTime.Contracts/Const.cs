namespace VinZ.Common;

public static  class TimeServiceConst
{
    private const string? Time = null;

    public static class Verb
    {
            public const string Set = $"{nameof(Time)}:{nameof(Set)}";
            public const string Freeze = $"{nameof(Time)}:{nameof(Freeze)}";
            public const string Unfreeze = $"{nameof(Time)}:{nameof(Unfreeze)}";
            public const string Reset = $"{nameof(Time)}:{nameof(Reset)}";
            public const string Forward = $"{nameof(Time)}:{nameof(Forward)}";
    }
}