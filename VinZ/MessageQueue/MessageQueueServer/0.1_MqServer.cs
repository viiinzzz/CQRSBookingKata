namespace VinZ.MessageQueue;

public partial class MqServer
(
    IScopeProvider scp,
    MqServerConfig config,
    ITimeService DateTime,
    ILogger<MqServer> log
)
    : IHostedLifecycleService
{
    private readonly bool _isTrace = config.IsTrace;

    private Task _executingTask = Task.CompletedTask;
    private CancellationTokenSource _executeCancel = new();

    private readonly bool _pauseOnError = config.PauseOnError;

    private void Check(LogLevel logLevel)
    {
        if (logLevel != LogLevel.Error || !_pauseOnError)
        {
            return;
        }

        Console.WriteLine(@"


Press a key to continue . . .


");
        Console.ReadKey();
    }

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

            if (dt >= _refresh)
            {
                RefreshFaster();

                continue;
            }

            var delay = _refresh - dt;

            if (delay < _refresh)
            {
                delay = _refresh;
            }

            RefreshSlower();

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