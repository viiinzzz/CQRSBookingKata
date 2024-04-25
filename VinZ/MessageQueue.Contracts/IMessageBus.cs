namespace VinZ.MessageQueue;

public interface IMessageBus
{
    void Subscribe(IMessageBusClient client, string? recipient, string? verb);
    bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb);

    INotifyAck Notify(IClientNotificationSerialized notification);

    Task<IClientNotificationSerialized?> Wait(INotifyAck ack, CancellationToken cancellationToken);
}