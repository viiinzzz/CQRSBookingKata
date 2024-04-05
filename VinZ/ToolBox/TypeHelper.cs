namespace VinZ.Common;

public static class TypeHelper
{
    public static Type[] Types<T1>() => new Type[] { typeof(T1) };
    public static Type[] Types<T1, T2>() => new Type[] { typeof(T1), typeof(T2) };
    public static Type[] Types<T1, T2, T3>() => new Type[] { typeof(T1), typeof(T2), typeof(T3) };
    public static Type[] Types<T1, T2, T3, T4>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
    public static Type[] Types<T1, T2, T3, T4, T5>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
}