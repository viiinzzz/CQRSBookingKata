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

public partial class DemoBus
{
    public int SimulationDay => demoContextService.SimulationDay;
    private const int DayMilliseconds = 24 * 60 * 1000;
    public const double SpeedFactorOneDayOneMinute = 24 * 60;
    private readonly object Fake_BookingDay_lock = new();

    public async Task<DateTime> Forward(int days, double? speedFactor, CancellationToken cancellationToken)
    {
        ClientNotification initialNotification = new RequestOptions
        {
            Recipient = nameof(Demo),
            Verb = nameof(Forward),
            Originator = originator
        };

        try
        {
            using var scope1 = sp.GetScope<IAdminRepository>(out var admin);
            using var scope2 = sp.GetScope<ISalesRepository>(out var sales);
            using var scope3 = sp.GetScope<IMoneyRepository>(out var money);
            using var scope4 = sp.GetScope<IGazetteerService>(out var geo);

            var context = new TransactionContext() * admin * money * sales * geo;

            for (var d = 0; d < days; d++)
            {
                var milliseconds = (int)(DayMilliseconds / (speedFactor ?? SpeedFactorOneDayOneMinute));
                if (milliseconds < 1000) milliseconds = 1000;

                await Task.Delay(milliseconds, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                DateTime.Forward(TimeSpan.FromDays(1));
                demoContextService.SimulationDay++;

                context.ExecuteExclusive(() => Fake_BookingDay(), Fake_BookingDay_lock);

                if (days == SeasonDayNumbers)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            bus.Notify(initialNotification.Response(new ResponseOptions {
                ex = ex,
                Immediate = true
            }));
        }

        return DateTime.UtcNow;
    }
}