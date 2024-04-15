namespace VinZ.MessageQueue;

public class MessageBus(IMessageBus server) : IMessageBus
{
    public Task<object?> Wait(INotifyAck ack, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Subscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        server.Subscribe(client, recipient, verb);
    }

    public bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        return server.Unsubscribe(client, recipient, verb);
    }

    public INotifyAck Notify(string originator, INotification notification)
    {
        return server.Notify(GetType().Name, notification);
    }

    public INotifyAck Notify(INotification notification)
    {
        return server.Notify(GetType().Name, notification);
    }
}