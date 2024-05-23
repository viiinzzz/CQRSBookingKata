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

using System.Linq;

namespace BookingKata.API.Demo;

public class DebugRoutesHostService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<EndpointDataSource> _log;

    private readonly IHostApplicationLifetime _lifetime;
    private readonly TaskCompletionSource _applicationStartedSource = new();

    public DebugRoutesHostService(
        IServiceProvider sp, 
        ILogger<EndpointDataSource> log,
        IHostApplicationLifetime lifetime
        )
    {
        _sp = sp;
        _log = log;

        _lifetime = lifetime;
        lifetime.ApplicationStarted.Register(() => _applicationStartedSource.SetResult());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tcs = new TaskCompletionSource();
        stoppingToken.Register(() => tcs.SetResult());

        await Task.WhenAny(tcs.Task, _applicationStartedSource.Task).ConfigureAwait(false);

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }


        var endpointSources = _sp.GetService<IEnumerable<EndpointDataSource>>() 
                              ?? throw new NullReferenceException();

        var debugRoutes = endpointSources

                .SelectMany(source => source.Endpoints)

                .Select(endpoint => new {
                    Pattern = (endpoint as RouteEndpoint)?.RoutePattern?.RawText,
                    Method = string.Join(',', endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods ?? [])
                })
                .SelectMany(r=> r.Method.Split(',').Select(m => r with {Method = m}))

                .OrderBy(r=> ((r.Pattern ?? string.Empty).StartsWith('/')  ? r.Pattern![1..] : r.Pattern)?.ToLower())
                .ThenBy(r => r.Method?.ToLower())

                .Select(r => $"{(r.Pattern.StartsWith('/') ? "" : '/')}{r.Pattern?.ToLower()} {r.Method}")

                .ToArray();

        _log.LogWarning($"{string.Join(Environment.NewLine, debugRoutes)}");
    }
}
