using System.Globalization;

namespace VinZ.Common;

public static class DateTimeHelper
{
    public static DateTime? DeserializeUniversal_ThrowIfNull(this string? dateTimeU, string argumentName)
    {
        if (dateTimeU == null)
        {
            throw new ArgumentNullException(argumentName);
        }

        return dateTimeU.DeserializeUniversal();
    }

    public static DateTime? DeserializeUniversal(this string? dateTimeU)
    {
        if (dateTimeU == null)
        {
            return null;
        }

        return DateTime
            .ParseExact(dateTimeU, "u", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }

    public static string SerializeUniversal(this DateTime dateTime)
    {
        return dateTime.ToString("u");
    }



    private static readonly long HalfSecondTicks = TimeSpan.TicksPerSecond / 2;

    public static DateTime RoundToTheSecond(this DateTime date)
    {
        var ret = date;

        var fractionalTicks = ret.Ticks % TimeSpan.TicksPerSecond;

        ret = ret.AddTicks(-fractionalTicks);

        if (fractionalTicks >= HalfSecondTicks)
        {
            ret = ret.AddTicks(TimeSpan.TicksPerSecond);
        }

        return ret;
    }


    public static DateTime DayStart(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
    }


    public static int DaysTo(this DateTime from, DateTime? to)
    {
        return (int)((to ?? System.DateTime.MaxValue) - from).TotalDays;
    }


    public record FromTo(DateTime From, DateTime? To)
    {
        public int DaysTo { get; set; }
    }

}


// using Microsoft.Linq.Translations;

//
//
// public static readonly CompiledExpression<FromTo, int> DaysToExpression =
//     DefaultTranslationOf<FromTo>.Property(fromTo => fromTo.DaysTo).Is(fromTo =>
//
//         (int)((fromTo.To ?? System.DateTime.MaxValue) - fromTo.From).TotalDays
//
//     );
// private static int EvaluateDaysTo(RoomServiceDuty duty)
// {
//     return DateTimeHelper.DaysToExpression.Evaluate(new DateTimeHelper.FromTo(duty.FreeTime, duty.Now));
// }

//
// private static Func<int> CompileSubtract<T>()
// {
//     var pFrom = Expression.Parameter(typeof(DateTime), "from");
//     var pTo = Expression.Parameter(typeof(DateTime?), "to");
//
//     var body = Expression.Subtract(pFrom, pTo);
//
//     return Expression.Lambda<Func<DateTime, DateTime, int>>(
//             body, pFrom, pTo
//             ).Compile();
// }
