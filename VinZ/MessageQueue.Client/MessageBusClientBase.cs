namespace VinZ.MessageQueue;

public class MessageBusClientBase : IMessageBusClient, IDisposable
{
    private MessageBusHttp? _bus;
    public ILogger<IMessageBus>? log { get; set; }

    private Func<Task>? pingOrThrow;

    public int PingTimeoutMilliseconds { get; set; } = 2_000;
    public int DisposeDelaySeconds { get; set; } = 120;
    public int RetryDelaySeconds { get; set; } = 5;
    public int RetryMaxCount { get; set; } = 10;
    public int SubscribeWarmupMilliseconds { get; set; } = 1000;


    // public IMessageBusClient ConnectToBus(IMessageBus bus)
    // {
    //     throw new NotImplementedException();
    // }

    public ILogger<IMessageBus>? Log { get; set; }

    private bool isTrace = true;

    public IMessageBusClient ConnectToBus(IScopeProvider scp)
    {
        var scope1 = scp.GetScope<BusConfiguration>(out var busConfig);
        var scope2 = scp.GetScope<ITimeService>(out var dateTime);
        var scope3 = scp.GetScope<ILogger<IMessageBus>>(out var log);

        isTrace = busConfig.IsTrace;
        var id = GetHashCode();
        var localUrl = $"{(busConfig.LocalUrl.EndsWith('/') ? busConfig.LocalUrl : busConfig.LocalUrl + '/')}{id.xby4()}";
        var remoteUrl = busConfig.RemoteUrl;

        _bus = new MessageBusHttp(busConfig, dateTime, log);

        var remoteHost = new Uri(remoteUrl).Host;

        var remoteIp = Dns.GetHostAddresses(remoteHost)
            .MinBy(a => a.AddressFamily == AddressFamily.InterNetwork ? 4 : a.AddressFamily == AddressFamily.InterNetworkV6 ? 6 : int.MaxValue);

        var ping = new Ping();

        pingOrThrow = async () =>
        {
            if (remoteIp == null)
            {
                throw new NullReferenceException(nameof(remoteIp));
            }

            var reply = await ping.SendPingAsync(remoteIp, PingTimeoutMilliseconds);

            var fail = reply.Status != IPStatus.Success;

            if (fail)
            {
                throw new InvalidOperationException($"No answer from <<<bus:{remoteHost}>>> ({remoteIp})");
            }
        };

        return this;
    }


    private async Task CheckBus()
    {
        if (_bus == null)
        {
            throw new InvalidOperationException("Not connected to a bus");
        }

        if (pingOrThrow != null)
        {
            await pingOrThrow();
        }

    }

    public record RetryOptions
    (
        int WarmupDelayMilliseconds = default
    );

    private async Task<T> Retry<T>(Func<T> action, RetryOptions? options = default)
    {
        var retryCount = 0;

        var t0 = DateTime.Now;

        if (options is { WarmupDelayMilliseconds: > 0 })
        {
            var dt = options.WarmupDelayMilliseconds;

            await Task.Delay(dt);
        }

        while (true)
        {
            retryCount++;

            try
            {
                var dt = RetryDelaySeconds * 1000;

                await Task.Delay(dt);

                await CheckBus();

                var t = action();

                return t;
            }
            catch (Exception ex)
            {
                var elapsedSeconds = (DateTime.Now - t0).TotalSeconds;

                if (retryCount >= RetryMaxCount)
                {
                    log?.LogError($"[{DateTime.Now:O}] Bus error: max retry reached: {ex.Message}");

                    throw new Exception($"Max retry reached ({retryCount})", ex);
                }

                log?.LogInformation($"      --> waiting bus... {elapsedSeconds:0.0}s ({retryCount}/{RetryMaxCount})");
            }
        }
    }

    public void Dispose()
    {
        if (_bus == null)
        {
            return;
        }

        Disconnect().Wait(DisposeDelaySeconds * 1000);
    }

    public async Task Disconnect()
    {
        await CheckBus();

        var done = await Unsubscribe(Omni, AnyVerb);

        _bus = null;
        pingOrThrow = null;

        if (!done)
        {
            throw new InvalidOperationException("Disconnection failure");
        }
    }


    public virtual async Task Configure()
    {

    }


    public async Task Subscribe(string? recipient = default, string? verb = default)
    {
        var task = await Retry(() =>
        {
            _bus!.Subscribe(new SubscriptionRequest
            {
                _type = $"{nameof(Subscribe)}",
                name = GetType().Name,
                url = _bus.Url.ToString(),
                recipient = recipient,
                verb =  verb
            }, 0);

            return true;
        }, new RetryOptions
        {
            WarmupDelayMilliseconds = SubscribeWarmupMilliseconds
        });
    }

    public async Task<bool> Unsubscribe(string? recipient = default, string? verb = default)
    {
        return await Retry(() =>
        {
            var done = _bus!.Unsubscribe(new SubscriptionRequest
            {
                _type = $"{nameof(Unsubscribe)}",
                name = GetType().Name,
                url = _bus.Url.ToString(),
                recipient = recipient,
                verb = verb
            }, 0);

            return done;
        });
    }


    public async Task<NotifyAck> Notify(IClientNotificationSerialized notification)
    {
        return await Retry(() =>
        {
            var ack = _bus!.Notify(notification, 0);

            return ack;
        });
    }

    public event EventHandler<IClientNotificationSerialized>? Notified;

    public virtual void OnNotified(IClientNotificationSerialized notification)
    {
        Notified?.Invoke(this, notification);
    }

    public TReturn? AskResult<TReturn>(string originator, string recipient, string verb, object? message)
    where TReturn : class
    {
        CheckBus();

        var ret = _bus!.AskResult<TReturn>(recipient, verb, message, originator);

        return ret;
    }

}
