namespace VinZ.MessageQueue;

public interface IMessageBus
{
    int Subscribe(IMessageBusClient client, string? recipient, string? verb);
    bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb);
    void Notify(INotifyMessage notifyMessage);
}
