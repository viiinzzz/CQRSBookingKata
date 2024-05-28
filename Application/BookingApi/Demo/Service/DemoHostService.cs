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

public class DemoHostService
    // : IHostedLifecycleService
    : BackgroundService
{
    private readonly string _debugSubscribeUrl;
    private readonly HashSet<string> _requiredParticipants;
    private readonly ILogger<DemoHostService> _log;
    private readonly TaskCompletionSource _applicationStartedSource = new();
    private readonly DemoBus _demo;

    public DemoHostService
    (
        MessageQueueConfiguration mqConfig,
        ILogger<DemoHostService> log,
        IHostApplicationLifetime lifetime,
        IServiceProvider sp
    )
    {

        var messageQueueUrl = mqConfig.messageQueueUrl == "self"
            ? $"{mqConfig.busUrl.Scheme}://{mqConfig.busUrl.Host}:{mqConfig.busUrl.Port}" 
            : mqConfig.messageQueueUrl;
        
        if (messageQueueUrl?.EndsWith('/') ?? false)
        {
            messageQueueUrl = messageQueueUrl[..^1];
        }

        var mqUri = new Uri(messageQueueUrl);

        _debugSubscribeUrl = $"{mqUri.Scheme}://{mqUri.Host}:{mqUri.Port}/debug/subscribe";


        _requiredParticipants = mqConfig.busTypes.Select(type => type.Name).ToHashSet();


        _log = log;

        lifetime.ApplicationStarted.Register(() => _applicationStartedSource.SetResult());


        using var scope = sp.CreateScope();

        _demo = scope.ServiceProvider.GetRequiredService<DemoBus>() ?? throw new NullReferenceException();
    }


    private const int DelayBeforeDemoStartSeconds = 20;

    protected async Task Execute(CancellationToken cancel)
    {
        //DI/Bus warmup delay before demo kicks in
        // await Task.Delay(DelayBeforeDemoStartSeconds * 1000, cancel);

        await _demo.Execute(cancel);
    }

    // public async Task<DateTime> Forward(int days, double? speedFactor, CancellationToken cancel) => await _demo.Forward(days, speedFactor, cancel);


    // private Task _executeTask = Task.CompletedTask;
    // private CancellationTokenSource _executeCancel = new();


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //wait application started
        var tcs = new TaskCompletionSource();
        stoppingToken.Register(() => tcs.SetResult());

        await Task.WhenAny(tcs.Task, _applicationStartedSource.Task).ConfigureAwait(false);

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }


        //wait bus composed
        var bigtimeout = new CancellationTokenSource(180000); //3min
        var bigcancel = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, bigtimeout.Token);

        var http = new HttpClient();

        (string url, string name)[] subscribe = [];
        bool ready = false;

        while (!bigcancel.IsCancellationRequested)
        {
            try
            {
                var timeout = new CancellationTokenSource(30000);
                var cancel = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

                //
                //
                var text = await http.GetStringAsync(_debugSubscribeUrl, cancel.Token);
                //
                //

                subscribe = text.Split('\n', StringSplitOptions.TrimEntries)
                    .Select(line =>
                    {
                        var row = line.Split(' ').Take(2).ToArray();

                        if (row.Length < 2)
                        {
                            return ((string)null, (string)null);
                        }

                        return (row[0], row[1]);
                    })
                    .Where(sub => sub.Item1 != null)
                    .ToArray();
            }
            catch (OperationCanceledException cancelEx)
            {
                ;
            }
            catch (Exception ex)
            {
                _log.LogWarning(@$"Failed to GET {_debugSubscribeUrl}
{ex.Message}");
            }
            var currentParticipants = subscribe.Select(s => s.name).ToHashSet();

            ready = currentParticipants.IsSupersetOf(_requiredParticipants);

            if (ready)
            {
                break;
            }

            var missingParticipants = _requiredParticipants.Except(currentParticipants);

            _log.LogWarning(@$"Waiting {missingParticipants.Count()} more required participant{(missingParticipants.Count() > 1 ? "s" : "")} to join the bus . . .
{string.Join(Environment.NewLine, currentParticipants.Select(p => $" - {p} Registered"))}
{string.Join(Environment.NewLine, missingParticipants.Select(p => $" - {p} Waiting"))}");

            await Task.Delay(5000, stoppingToken);
        }

        if (!ready)
        {
            _log.LogError("Give up waiting all required participants to join the bus.");

            return;
        }

        //let's demo
        await Execute(stoppingToken);
    }

    // public async Task StartAsync(CancellationToken cancellationToken)
    // {
    //     _executeTask = Execute(_executeCancel.Token);
    // }
    //
    // public async Task StartingAsync(CancellationToken cancellationToken)
    // {
    // }
    //
    // public async Task StartedAsync(CancellationToken cancellationToken)
    // {
    // }
    //
    // public async Task StopAsync(CancellationToken cancellationToken)
    // {
    //     _executeCancel.Cancel();
    //
    //     await Task.WhenAny(_executeTask, Task.Delay(Timeout.Infinite, cancellationToken));
    // }
    //
    // public async Task StoppingAsync(CancellationToken cancellationToken)
    // {
    // }
    //
    // public async Task StoppedAsync(CancellationToken cancellationToken)
    // {
    // }
}
