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

namespace BookingKata.API.Demo;

public partial class DemoService
{
    public int SimulationDay => demoContext.SimulationDay;
    private const int DayMilliseconds = 24 * 60 * 1000;
    public const double SpeedFactorOneDayOneMinute = 24 * 60;
    private readonly object Fake_BookingDay_lock = new();

    public async Task<DateTime> Forward(int days, double? speedFactor, CancellationToken cancellationToken)
    {
        try
        {
            var context = new TransactionContext() * admin * money * sales * geo;

            for (var d = 0; d < days; d++)
            {
                var milliseconds = (int)(DayMilliseconds / (speedFactor ?? SpeedFactorOneDayOneMinute));
                if (milliseconds < 1000) milliseconds = 1000;

                await Task.Delay(milliseconds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                DateTime.Forward(TimeSpan.FromDays(1));
                demoContext.SimulationDay++;

                context.ExecuteExclusive(() => Fake_BookingDay(), Fake_BookingDay_lock);

                if (days == SeasonDayNumbers)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            var childNotification = new RequestNotification(nameof(Demo), nameof(Forward));

            bus.Notify(new NegativeResponseNotification(childNotification, ex, "aborted!")
            {
                Originator = originator,
                Immediate = true
            });
        }

        return DateTime.UtcNow;
    }
}