namespace VinZ.MessageQueue;

public interface IMessageBusClient
{
    // IMessageBusClient ConnectToBus(IMessageBus bus);
    public IMessageBusClient ConnectToBus(IScopeProvider scp);


    Task Disconnect();

    Task Configure();

    Task Subscribe(string? recipient, string? verb);
    Task<bool> Unsubscribe(string? recipient, string? verb);

    Task Notify(IClientNotificationSerialized notification);
    void OnNotified(IClientNotificationSerialized notification);

    event EventHandler<IClientNotificationSerialized>? Notified;
}