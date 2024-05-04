namespace VinZ.MessageQueue;

public interface IMessageBus
{
    // void Subscribe(IMessageBusClient client, string? recipient, string? verb);
    // bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb);
    void Subscribe(SubscriptionRequest sub, int busId = 0);
    bool Unsubscribe(SubscriptionRequest sub, int busId = 0);

    NotifyAck Notify(IClientNotificationSerialized notification, int busId = 0);

    Task<IClientNotificationSerialized?> Wait(NotifyAck ack, CancellationToken cancellationToken);
}


public record SubscriptionRequest
(
    string _type = default,
    string name = default,
    string url = default,
    string? recipient = default,
    string? verb = default
);