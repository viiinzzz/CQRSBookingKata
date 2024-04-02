using Newtonsoft.Json;

namespace VinZ.MessageQueue;

public partial class MessageQueueServer
{
    public void Notify(INotifyMessage notifyMessage) => Notify(notifyMessage, CancellationToken.None); //fire and forget

    public async Task Notify(INotifyMessage notifyMessage, CancellationToken cancel)
    {
        var now = DateTime.UtcNow;

        var n = notifyMessage;

        if (n.Message == default && n.Verb == default && n.Recipient == default)
        {
            return;
        }

        var immediate = n.Immediate ?? false;
        var selectEarliest = !immediate && n is { EarliestDelivery: { Ticks: > 0 } };
        var selectLatest = !immediate && n is { LatestDelivery: { Ticks: > 0 } };
        var selectRepeat = n is { RepeatDelay: { Ticks: > 0 }, RepeatCount: > 0 };
        var now2 = immediate && selectRepeat ? now + n.RepeatDelay.Value : now;

        var notification = new ServerNotification
        {
            Json = JsonConvert.SerializeObject(n.Message),
            Verb = n.Verb,
            Recipient = n.Recipient,

            MessageTime = now,
            EarliestDelivery = selectEarliest ? new[] { now2, now + n.EarliestDelivery.Value }.Max() : now,
            LatestDelivery = selectLatest ? now + n.LatestDelivery.Value : System.DateTime.MaxValue,
            RepeatDelay = n.RepeatDelay ?? TimeSpan.Zero,
            RepeatCount = !selectRepeat ? 0 : immediate ? n.RepeatCount.Value - 1 : n.RepeatCount.Value,
            Aggregate = n.Aggregate ?? false,
        };

        if (immediate)
        {
            await Broadcast(notification, true, cancel);
        }

        if (!immediate || notification.RepeatCount > 0)
        {
            using var scope = scp.GetScope<IMessageQueueRepository>(out var queue);


            var queuing = immediate ? "                     <<<Relaying<<< immediate" : "                      <<<Queuing<<< scheduled";

            Console.Out.WriteLine(
                $"{queuing} message{(immediate ? "" : "Id:" + notification.MessageId)}... {{recipient:{notification.Recipient}, verb:{notification.Verb}, message:{notification.Json.Replace("\"", "")}}}");

            queue.AddNotification(notification);
        }
    }
}