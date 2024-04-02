﻿namespace VinZ.FakeTime;

public static class Verb
{
    public static class Time
    {
        public const string Set = $"{nameof(Time)}:{nameof(Set)}";
        public const string Freeze = $"{nameof(Time)}:{nameof(Freeze)}";
        public const string Unfreeze = $"{nameof(Time)}:{nameof(Unfreeze)}";
        public const string Reset = $"{nameof(Time)}:{nameof(Reset)}";
        public const string Forward = $"{nameof(Time)}:{nameof(Forward)}";
    }
}