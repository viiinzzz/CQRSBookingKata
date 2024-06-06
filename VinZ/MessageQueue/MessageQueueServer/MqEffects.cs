namespace VinZ.MessageQueue;

public class MqEffects
{
    public event EventHandler<ServerNotificationChange>? OnNotified;

    public virtual void Notify(ServerNotification notification)
    {
        OnNotified?.Invoke(this, new ServerNotificationChange(notification));
    }
}