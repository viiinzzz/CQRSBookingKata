namespace VinZ.MessageQueue;

public interface IMessageBus
{
    void Subscribe(IMessageBusClient client, string? recipient, string? verb);
    bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb);
    INotifyAck Notify(string originator, INotifyMessage notifyMessage);
}