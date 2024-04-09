namespace VinZ.MessageQueue;

public static class IClientNotificationHelper
{
    public static CorrelationId CorrelationId(this IClientNotification notification)
    {
        return new CorrelationId(notification.CorrelationId1, notification.CorrelationId2);
    } 

    public static string CorrelationGuid(this IClientNotification notification)
    {
        return CorrelationId(notification).Guid;
    }
}