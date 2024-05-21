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

public class DemoHostService : IHostedLifecycleService
{
    private readonly DemoBus _demo;

    public DemoHostService(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();

        _demo = scope.ServiceProvider.GetRequiredService<DemoBus>() ?? throw new NullReferenceException();
    }


    protected async Task Execute(CancellationToken cancel)
    {
        await _demo.Execute(cancel);
    }

    // public async Task<DateTime> Forward(int days, double? speedFactor, CancellationToken cancel) => await _demo.Forward(days, speedFactor, cancel);


    private Task _executeTask = Task.CompletedTask;
    private CancellationTokenSource _executeCancel = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _executeTask = Execute(_executeCancel.Token);
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _executeCancel.Cancel();

        await Task.WhenAny(_executeTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
    }

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
    }
}
