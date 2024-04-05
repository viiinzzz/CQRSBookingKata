namespace BookingKata.API.Demo;

public class DemoHostService : IHostedLifecycleService
{
    private readonly DemoService _demo;

    public DemoHostService(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();

        _demo = scope.ServiceProvider.GetRequiredService<DemoService>() ?? throw new NullReferenceException();
    }


    protected async Task Execute(CancellationToken cancel) => await _demo.Execute(cancel);

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
