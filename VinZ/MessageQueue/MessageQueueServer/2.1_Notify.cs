namespace VinZ.MessageQueue;

public partial class MqServer
{
    public INotifyAck Notify(IClientNotificationSerialized notification)
    {
        return Notify(notification, CancellationToken.None);
    }

    public INotifyAck Notify(IClientNotificationSerialized clientNotification, CancellationToken cancel)
    {
        var now = DateTime.UtcNow;

        var n = clientNotification;

        var immediate = n.Immediate ?? false;
        var selectEarliest = !immediate && n is { EarliestDelivery: { Ticks: > 0 } };
        var selectLatest = !immediate && n is { LatestDelivery: { Ticks: > 0 } };
        var selectRepeat = n is { RepeatDelay: { Ticks: > 0 }, RepeatCount: > 0 };
        var now2 = immediate && selectRepeat ? now + n.RepeatDelay.Value : now;

        var correlationId = 
            n is { CorrelationId1: 0, CorrelationId2: 0 }
                ? CorrelationId.New()
                : new CorrelationId(n.CorrelationId1, n.CorrelationId2);

        var originator = clientNotification.Originator;

        var notification = new ServerNotification
        {
            Type = n.Type,
            Message = n.Message,
            MessageType = n.MessageType,

            Verb = n.Verb,
            Recipient = n.Recipient,
            Status = n.Status,

            Originator = originator,
            CorrelationId1 = correlationId.Id1,
            CorrelationId2 = correlationId.Id2,

            NotificationTime = now,
            EarliestDelivery = selectEarliest ? new[] { now2, now + n.EarliestDelivery.Value }.Max() : now,
            LatestDelivery = selectLatest ? now + n.LatestDelivery.Value : System.DateTime.MaxValue,
            RepeatDelay = n.RepeatDelay ?? TimeSpan.Zero,
            RepeatCount = !selectRepeat ? 0 : immediate ? n.RepeatCount.Value - 1 : n.RepeatCount.Value,
            Aggregate = n.Aggregate ?? false,
        };

        var ack = new NotifyAck
        {
            Valid = true,
            CorrelationId = correlationId
        };

        if (immediate)
        {
            //
            //
            Respond(notification, immediate: true, cancel);
            //
            //

            return ack;
        }

        if (!immediate || notification.RepeatCount > 0)
        {
            using var scope = scp.GetScope<IMessageQueueRepository>(out var queue);


            var queuing = immediate ? "                     <<<Relaying<<< Immediate" : "                      <<<Queuing<<< Scheduled";
            var notificationLabel = $"Notification{correlationId.Guid}{(immediate ? "" : $" (Id:?)")}";
            var rvm = @$"
  {{recipient: ""{notification.Recipient}"", verb: ""{notification.Verb}"", message: {notification.Message}}}";
            var logLevel = notification.IsErrorStatus() ? LogLevel.Error : LogLevel.Information;

            log.Log(logLevel, @$"{queuing} {notificationLabel} ... {rvm}");

            //
            //
            queue.AddNotification(notification);
            //
            //

            return ack;
        }

        return new NotifyAck
        {
            Valid = false
        };
    }
}