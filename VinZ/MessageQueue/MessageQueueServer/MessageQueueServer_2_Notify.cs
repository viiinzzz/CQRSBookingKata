using Newtonsoft.Json;

namespace VinZ.MessageQueue;

public partial class MessageQueueServer
{
    public INotifyAck Notify(string originator, INotifyMessage notifyMessage)
    {
        return Notify(originator, notifyMessage, CancellationToken.None);
    }

    public INotifyAck Notify(string originator, INotifyMessage notifyMessage, CancellationToken cancel)
    {
        var now = DateTime.UtcNow;

        var n = notifyMessage;

        var immediate = n.Immediate ?? false;
        var selectEarliest = !immediate && n is { EarliestDelivery: { Ticks: > 0 } };
        var selectLatest = !immediate && n is { LatestDelivery: { Ticks: > 0 } };
        var selectRepeat = n is { RepeatDelay: { Ticks: > 0 }, RepeatCount: > 0 };
        var now2 = immediate && selectRepeat ? now + n.RepeatDelay.Value : now;

        var correlationId = 
            !n.CorrelationId1.HasValue ||
            !n.CorrelationId2.HasValue
                ? CorrelationId.New()
                : new CorrelationId(n.CorrelationId1.Value, n.CorrelationId2.Value);

        var notification = new ServerNotification
        {
            Json = n.Message == default ? "{}" : JsonConvert.SerializeObject(n.Message),
            Verb = n.Verb,
            Recipient = n.Recipient,

            Originator = originator,
            CorrelationId1 = correlationId.Id1,
            CorrelationId2 = correlationId.Id2,

            MessageTime = now,
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
            Broadcast(notification, immediate: true, cancel);
            //
            //

            return ack;
        }

        if (!immediate || notification.RepeatCount > 0)
        {
            using var scope = scp.GetScope<IMessageQueueRepository>(out var queue);


            var queuing = immediate ? "                     <<<Relaying<<< immediate" : "                      <<<Queuing<<< scheduled";

            log.LogInformation(
                @$"{queuing} message{(immediate ? "" : "Id:" + notification.MessageId)}...
{{recipient:{notification.Recipient}, verb:{notification.Verb}, message:{notification.Json.Replace("\"", "")}}}");

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