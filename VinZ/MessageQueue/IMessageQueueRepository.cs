namespace VinZ.MessageQueue;

public interface IMessageQueueRepository
{
    IQueryable<ServerNotification> Notifications { get; }

    void AddNotification(ServerNotification notification);
    int UpdateNotification(IEnumerable<ServerNotification> notifications, ServerNotificationUpdate update, bool scoped);
    int ArchiveNotifications();
}