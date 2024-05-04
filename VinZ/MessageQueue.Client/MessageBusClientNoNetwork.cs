namespace VinZ.MessageQueue;

public class MessageBusClientNoNetwork : IMessageBusClient //ancien MessageBusClientBase
{
    private IMessageBus? _bus;

    private void CheckBus()
    {
        if (_bus == null)
        {
            throw new InvalidOperationException("Not connected to a bus");
        }
    }


    // public IMessageBusClient ConnectToBus(IMessageBus bus)
    // {
    //     if (_bus != null)
    //     {
    //         throw new InvalidOperationException("Already connected to a bus");
    //     }
    //
    //     _bus = bus;
    //
    //     return this;
    // }

    public ILogger<IMessageBus>? Log { get; set; }

    public IMessageBusClient ConnectToBus(IScopeProvider scp)
    {
        if (_bus != null)
        {
            throw new InvalidOperationException("Already connected to a bus");
        }

        var scope = scp.GetScope<IMessageBus>(out var bus);

        _bus = bus;

        return this;
    }

    public async Task Disconnect()
    {
        CheckBus();


        var done = await Unsubscribe(Omni, AnyVerb);

        _bus = null;

        if (!done)
        {
            throw new InvalidOperationException("Disconnection failure");
        }
    }


    public virtual async Task Configure() { }


    public Task Subscribe(string? recipient = default, string? verb = default)
    {
        CheckBus();

        _bus!.Subscribe(new SubscriptionRequest
        {
            _type = $"{nameof(Subscribe)}",
            name = null,
            url = null,
            recipient = recipient,
            verb = verb
        });

        return Task.CompletedTask;
    }

    public Task<bool> Unsubscribe(string? recipient = default, string? verb = default)
    {
        CheckBus();

        var done = _bus!.Unsubscribe(new SubscriptionRequest
        {
            _type = $"{nameof(Unsubscribe)}",
            name = null,
            url = null,
            recipient = recipient,
            verb = verb
        });

        return Task.FromResult(done);
    }


    public Task<NotifyAck> Notify(IClientNotificationSerialized notification)
    {
        CheckBus();

        _bus!.Notify(notification);

        return Task.FromResult(new NotifyAck
        {
            Valid = true,
            Status = 0,
            data = default,
            CorrelationId = notification.CorrelationId()
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