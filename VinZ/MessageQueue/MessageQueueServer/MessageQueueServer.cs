namespace VinZ.MessageQueue;

public partial class MessageQueueServer
(
    IScopeProvider scp,
    MessageQueueServerConfig config,
    ITimeService DateTime,
    ILogger<MessageQueueServer> log
)
    : IHostedLifecycleService
{
    private Task _executingTask = Task.CompletedTask;
    private CancellationTokenSource _executeCancel = new();


    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        //initialize
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _executeCancel = new CancellationTokenSource();

        _executingTask = Execute(_executeCancel.Token);
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        //executing
    }

    private async Task Execute(CancellationToken cancel)
    {
        var refresh = config.BusRefreshMinMilliseconds;
        var logRefresh = () => log.LogInformation($"Throttling refresh rate @{refresh}ms");

        while (!cancel.IsCancellationRequested)
        {
            var t0 = DateTime.UtcNow;

            //
            //
            await ProcessQueue(cancel);
            //
            //

            var t1 = DateTime.UtcNow;

            var dt = (int)Math.Ceiling((t1 - t0).TotalMilliseconds);

            if (dt >= refresh)
            {
                if (refresh / 2 >= config.BusRefreshMinMilliseconds)
                {
                    refresh /= 2;
                    logRefresh();
                }

                continue;
            }

            var delay = refresh - dt;

            if (delay < refresh)
            {
                delay = refresh;
            }

            if (refresh * 2 <= config.BusRefreshMaxMilliseconds)
            {
                refresh *= 2;
                logRefresh();
            }

            await Task.Delay(delay, cancel);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _executeCancel.Cancel();

        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        //tearing down
    }

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        //tear down
    }

}