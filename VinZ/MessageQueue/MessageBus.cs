namespace VinZ.MessageQueue;

public class MessageBus(IMessageBus server) : IMessageBus
{
    public Task<TReturn> Wait<TReturn>(INotifyAck ack, string recipient, string respondVerb,
        CancellationToken cancellationToken)

        => throw new NotImplementedException();

    public void Subscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        server.Subscribe(client, recipient, verb);
    }

    public bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        return server.Unsubscribe(client, recipient, verb);
    }

    public INotifyAck Notify(string originator, INotifyMessage notifyMessage)
    {
        return server.Notify(GetType().Name, notifyMessage);
    }

    public INotifyAck Notify(INotifyMessage notifyMessage)
    {
        return server.Notify(GetType().Name, notifyMessage);
    }
}