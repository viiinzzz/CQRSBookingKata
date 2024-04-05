namespace VinZ.Common;

public static class RandomHelper
{
    private static readonly System.Random random = new();

    public static int Rand(int max) => random.Next(max);
    public static long Rand(long max) => random.NextInt64(max);
    public static double Rand(double max) => random.NextDouble() * max;

    public static int Int() => random.Next();
    public static long Long() => random.NextInt64();
    public static (long, long) Guid()
    {
        var guid = System.Guid.NewGuid();

        return guid.ToLong2();
    }

    public static string ToGuidB(this (long, long) twoLongs)
    {
        return new[] { twoLongs.Item1, twoLongs.Item2 }.ToGuid().ToString("B");

    }
    public static Guid ToGuid(this long[] twoLongs)
    {
        var l = twoLongs;

        return new Guid(
            (int)l[0], (short)(l[0] >> 32), (short)(l[0] >> 48),
            (byte)l[1], (byte)(l[1] >> 8), (byte)(l[1] >> 16), (byte)(l[1] >> 24),
            (byte)(l[1] >> 32), (byte)(l[1] >> 40), (byte)(l[1] >> 48), (byte)(l[1] >> 56)
        );
    }

    public static (long, long) ToLong2(this Guid guid)
    {
        var bytes = guid.ToByteArray();

        long l0 = 0;
        long l1 = 0;

        for (var i = 0; i < 8; i++)
        {
            l0 |= (long)bytes[i] << (8 * i);
        }

        for (var i = 8; i < 16; i++)
        {
            l1 |= (long)bytes[i] << (8 * i);
        }

        return (l0, l1);
    }

}