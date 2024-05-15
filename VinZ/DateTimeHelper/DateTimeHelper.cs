/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Globalization;

namespace VinZ.Common;

public static class DateTimeHelper
{
    public static DateTime DeserializeUniversal_ThrowIfNull(this string? dateTimeU, string argumentName)
    {
        if (string.IsNullOrEmpty(dateTimeU))
        {
            throw new ArgumentNullException(argumentName);
        }

        var ret = dateTimeU.DeserializeUniversal();

        return (DateTime)ret!;
    }

    public static DateTime? DeserializeUniversal(this string? dateTimeU, DateTime? defaultValue = null)
    {
        if (dateTimeU == null)
        {
            return defaultValue;
        }

        return DateTime
            .ParseExact(dateTimeU, "u", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }

    public static string? SerializeUniversal(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return null;
        }

        return dateTime!.Value.SerializeUniversal();
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
