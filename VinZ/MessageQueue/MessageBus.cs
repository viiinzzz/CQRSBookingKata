namespace VinZ.MessageQueue;

public class MessageBus(IMessageBus server) : IMessageBus
{
    public Task<IClientNotificationSerialized?> Wait(INotifyAck ack, CancellationToken cancellationToken)
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

    public INotifyAck Notify(IClientNotificationSerialized notification)
    {
        return server.Notify(notification);
    }
}