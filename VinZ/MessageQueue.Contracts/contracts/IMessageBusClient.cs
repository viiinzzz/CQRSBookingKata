namespace VinZ.MessageQueue;

public interface IMessageBusClient
{
    ILogger<IMessageBus>? Log { get; set; }

    // IMessageBusClient ConnectToBus(IMessageBus bus);
    IMessageBusClient ConnectToBus(IScopeProvider scp);


    Task Disconnect();

    Task Configure();

    Task Subscribe(string? recipient, string? verb);
    Task<bool> Unsubscribe(string? recipient, string? verb);

    Task<NotifyAck> Notify(IClientNotificationSerialized notification);
    void OnNotified(IClientNotificationSerialized notification);

    event EventHandler<IClientNotificationSerialized>? Notified;
}