namespace VinZ.MessageQueue;

public interface IMessageBus
{
    // void Subscribe(IMessageBusClient client, string? recipient, string? verb);
    // bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb);
    void Subscribe(SubscriptionRequest sub, int busId = 0);
    bool Unsubscribe(SubscriptionRequest sub, int busId = 0);

    INotifyAck Notify(IClientNotificationSerialized notification, int busId = 0);

    Task<IClientNotificationSerialized?> Wait(INotifyAck ack, CancellationToken cancellationToken);
}


public record SubscriptionRequest
(
    string name = default,
    string url = default,
    string? recipient = default,
    string? verb = default
);