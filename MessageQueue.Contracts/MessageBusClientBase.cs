namespace Vinz.MessageQueue;

public class MessageBusClientBase(IMessageBus bus) : IMessageBusClient
{
    public int Subscribe(string? recipient, string? verb) => bus.Subscribe(this, recipient, verb);
    public bool Unsubscribe(string? recipient, string? verb) => bus.Unsubscribe(this, recipient, verb);

    public void Notify(INotifyMessage message) => bus.Notify(message);

    public event EventHandler<IClientMessage>? Notified;

    public virtual void OnNotified(IClientMessage message)
    {
        Notified?.Invoke(this, message);
    }

}
