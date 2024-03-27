namespace VinZ.GeoIndexing;

public static partial class S2GeometryHelper
{
    public static (byte, byte) S2MinMaxLevelForKm(double? minKm, double? maxKm)
    {
        minKm ??= 0;
        maxKm ??= 10_000;

        if (minKm >= maxKm)
        {
            throw new ArgumentException($"must be less than ${nameof(maxKm)}", nameof(minKm));
        }

        int maxLevel, minLevel;

        for (maxLevel = S2Levels.Length - 1; maxLevel >= 0; maxLevel--)
        {
            var minKmLo = S2Level2MinKm[maxLevel];

            if (minKm > minKmLo) continue;

            // if (maxLevel > 0) maxLevel--;

            break;
        }

        for (minLevel = 0; minLevel <= S2Levels.Length - 1; minLevel++)
        {
            var maxKmHi = S2Level2MaxKm[minLevel];

            if (maxKm < maxKmHi) continue;

            // if (minLevel < S2Levels.Length - 1) minLevel++;
            if (minLevel > 0) minLevel--;

            break;
        }

        return ((byte)minLevel, (byte)maxLevel);
    }


    public static (double, double) EarthMinMaxKmForS2Level(this byte s2Level)
    {
        return (s2Level.EarthMinKmForS2Level(), s2Level.EarthMaxKmForS2Level());
    }


    private static int[] S2Levels = 
        0.RangeTo(30)
            .ToArray();

    private static double[] S2Level2MinKm =
        S2Levels
            .Select(EarthMinKmForS2Level_)
            .ToArray();

    private static double[] S2Level2MaxKm =
        S2Levels
            .Select(EarthMaxKmForS2Level_)
            .ToArray();


    public static double EarthMinKmForS2Level(this byte s2Level)
    {
        return s2Level switch
        {
            <= 30 => S2Level2MinKm[s2Level],
            _ => S2Level2MinKm[^1],
        };
    }

    public static double EarthMaxKmForS2Level(this byte s2Level)
    {
        return s2Level switch
        {
            <= 30 => S2Level2MaxKm[s2Level],
            _ => S2Level2MaxKm[^1],
        };
    }


    private static double EarthMinKmForS2Level_(this int s2Level)
    {
        return s2Level switch
        {
            00 => 7_842,
            01 => 3_921,
            02 => 1_825,
            <= 30 => 840 * Math.Pow(0.5, s2Level - 3),
            _ => 5120 * Math.Pow(0.5, 27),
        };
    }

    private static double EarthMaxKmForS2Level_(this int s2Level)
    {
        return s2Level switch
        {
            00 => 7_842,
            <= 30 => 5120 * Math.Pow(0.5, s2Level - 1),
            _ => 5120 * Math.Pow(0.5, 29),
        };
    }
}
