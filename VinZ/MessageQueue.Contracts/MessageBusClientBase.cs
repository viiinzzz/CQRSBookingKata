namespace VinZ.MessageQueue;


public class MessageBusClientBase : IMessageBusClient
{
    private IMessageBus? _bus;

    private void CheckBus()
    {
        if (_bus == null)
        {
            throw new InvalidOperationException("Not connected to a bus");
        }
    }

    public IMessageBusClient ConnectTo(IMessageBus bus)
    {
        if (_bus != null)
        {
            throw new InvalidOperationException("Already connected to a bus");
        }

        _bus = bus;

        return this;
    }

    public virtual void Configure() { }

    public bool Disconnect()
    {
        CheckBus();


        var ret = Unsubscribe(Bus.Any, Verb.Any);

        _bus = null;

        return ret;
    }

    public int Subscribe(string? recipient = default, string? verb = default)
    {
        CheckBus();

        return _bus!.Subscribe(this, recipient, verb);
    }

    public bool Unsubscribe(string? recipient = default, string? verb = default)
    {
        CheckBus();

        return _bus!.Unsubscribe(this, recipient, verb);
    }

    public void Notify(INotifyMessage message)
    {
        CheckBus();

        _bus!.Notify(message);
    }

    public event EventHandler<IClientNotification>? Notified;

    public virtual void OnNotified(IClientNotification notification)
    {
        Notified?.Invoke(this, notification);
    }

}
