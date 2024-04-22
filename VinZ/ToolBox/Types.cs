namespace VinZ.Common;

public static class Types
{
    public static Type[] From<T1>() => new Type[] { typeof(T1) };
    public static Type[] From<T1, T2>() => new Type[] { typeof(T1), typeof(T2) };
    public static Type[] From<T1, T2, T3>() => new Type[] { typeof(T1), typeof(T2), typeof(T3) };
    public static Type[] From<T1, T2, T3, T4>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
    public static Type[] From<T1, T2, T3, T4, T5>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
    public static Type[] From<T1, T2, T3, T4, T5, T6>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
    public static Type[] From<T1, T2, T3, T4, T5, T6, T7>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
    public static Type[] From<T1, T2, T3, T4, T5, T6, T7, T8>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
    public static Type[] From<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
}