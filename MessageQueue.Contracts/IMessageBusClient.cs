namespace Vinz.MessageQueue;

public interface IMessageBusClient
{
    int Subscribe(string? recipient, string? verb);
    bool Unsubscribe(string? recipient, string? verb);
    void Notify(INotifyMessage message);

    void OnNotified(IClientMessage message);
}