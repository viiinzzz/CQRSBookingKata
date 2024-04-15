namespace VinZ.MessageQueue;



public interface IMessageBusClient
{
    IMessageBusClient ConnectToBus(IMessageBus bus);
    bool Disconnect();

    void Configure();

    void Subscribe(string? recipient, string? verb);
    bool Unsubscribe(string? recipient, string? verb);

    void Notify(INotification message);
    void OnNotified(IClientNotification notification);

    event EventHandler<IClientNotification>? Notified;
}