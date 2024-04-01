namespace VinZ.MessageQueue;

public interface IMessageBusClient
{
    IMessageBusClient ConnectTo(IMessageBus bus);

    void Configure();

    bool Disconnect();

    int Subscribe(string? recipient, string? verb);
    bool Unsubscribe(string? recipient, string? verb);
    void Notify(INotifyMessage message);

    void OnNotified(IClientNotification notification);
}