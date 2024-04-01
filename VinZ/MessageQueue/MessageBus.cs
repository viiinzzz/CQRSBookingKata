namespace VinZ.MessageQueue;

public class MessageBus(IMessageBus server) : IMessageBus
{
    public int Subscribe(IMessageBusClient client, string? recipient, string? verb) => server.Subscribe(client, recipient, verb);
    public bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb) => server.Unsubscribe(client, recipient, verb);
    public void Notify(INotifyMessage notifyMessage) => server.Notify(notifyMessage);
}