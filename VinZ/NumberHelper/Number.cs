using System.Linq.Expressions;
using System.Reflection;

namespace VinZ.Number;

//https://stackoverflow.com/questions/40900594/mapping-a-ulong-to-a-long-in-c

public static class Number<TNum>
{
    public static TNum MinValue = GetMinValue<TNum>();
    public static TNum MaxValue = GetMaxValue<TNum>();

    public static Func<TNum, TNum, TNum> Add = CompileAdd<TNum>();
    public static Func<TNum, TNum, TNum> Subtract = CompileSubtract<TNum>();


    private static object GetConstValue(Type t, string propertyName)
    {
        var pi = t.GetField(propertyName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        return pi.GetValue(null);
    }

    private static T GetMinValue<T>() => (T)GetConstValue(typeof(T), "MinValue");
    private static T GetMaxValue<T>() => (T)GetConstValue(typeof(T), "MaxValue");


    private static Func<T, T, T> CompileAdd<T>()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");

        var body = Expression.Add(paramA, paramB);

        var add = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB)
            .Compile();

        return add;
    }


    private static Func<T, T, T> CompileSubtract<T>()
    {
        var paramA = Expression.Parameter(typeof(T), "a");
        var paramB = Expression.Parameter(typeof(T), "b");

        var body = Expression.Subtract(paramA, paramB);

        var subtract = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB)
            .Compile();

        return subtract;
    }
}


public static class NumberHelper
{
    public static TSigned MapUnsignedToSigned<TUnsigned, TSigned>(this TUnsigned ulongValue)
    {
        var signed = default(TSigned);

        unchecked
        {
            signed = Number<TSigned>.Add(
                (TSigned)(dynamic)ulongValue,
                Number<TSigned>.MinValue);
        }
        return signed;
    }


    public static TUnsigned MapSignedToUnsigned<TSigned, TUnsigned>(this TSigned longValue)
    {
        var unsigned = default(TUnsigned);

        unchecked
        {
            unsigned = (TUnsigned)(dynamic)Number<TSigned>.Subtract(
                longValue,
                Number<TSigned>.MinValue);
        }

        return unsigned;
    }
}
