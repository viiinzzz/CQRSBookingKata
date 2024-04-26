namespace VinZ.Common;

public static  class TimeServiceConst
{
    public const string Time = nameof(Time);

    public static class Verb
    {
            public const string Set = $"{Time}:{nameof(Set)}";
            public const string Freeze = $"{Time}:{nameof(Freeze)}";
            public const string Unfreeze = $"{Time}:{nameof(Unfreeze)}";
            public const string Reset = $"{Time}:{nameof(Reset)}";
            public const string Forward = $"{Time}:{nameof(Forward)}";
    }
}