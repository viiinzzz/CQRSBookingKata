using Microsoft.Extensions.Logging;

namespace VinZ.MessageQueue;

public partial class MessageQueueServer
{
    private (DeliveryCount, List<(ServerNotification[], ServerNotificationUpdate)>) Broadcast(ServerNotification notification, bool immediate,
        CancellationToken cancel)
    {
        var updates = new List<(ServerNotification[], ServerNotificationUpdate)>();

        var subscribers = new List<IMessageBusClient>();

        if (notification.Recipient == default && notification.Verb == default)
        {
            var moreSubscribers = _subscribers_0.Values;

            subscribers.AddRange(moreSubscribers);
        }
        else if (notification.Recipient != default && notification.Verb == default)
        {
            if (_subscribers_R.TryGetValue(notification.Recipient.GetHashCode(), out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }
        else if (notification.Recipient == default && notification.Verb != default)
        {
            if (_subscribers_V.TryGetValue(notification.Verb.GetHashCode(), out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }
        else if (notification.Recipient != default && notification.Verb != default)
        {
            if (_subscribers_RV.TryGetValue((notification.Recipient, notification.Verb).GetHashCode(),
                    out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }

        var dequeuing = immediate ? "            >>>Relaying>>>          immediate" : ">>>Dequeuing>>>                     scheduled";

        log.LogInformation(@$"{dequeuing} message{(immediate ? "" : "Id:" + notification.MessageId)
        } to {subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")
        }...
  {{recipient:{notification.Recipient}, verb:{notification.Verb
        }, message:{notification.Json.Replace("\"", "")}}}");

        var updateMessage = () =>
        {
            if (notification is { RepeatDelay: { Ticks: > 0 }, RepeatCount: > 1 })
            {
                updates.Add((new[] { notification }, new ServerNotificationUpdate
                {
                    RepeatCount = notification.RepeatCount - 1
                }));
            }
            else
            {
                updates.Add((new[] { notification }, new ServerNotificationUpdate
                {
                    Done = true,
                    DoneTime = DateTime.UtcNow
                }));
            }
        };

        DeliveryCount count = default;

        if (subscribers.Count == 0)
        {
            if (!immediate) updateMessage();

            return (count, updates);
        }

        var clientMessage = new ClientNotification
        {
            Json = notification.Json,
            Recipient = notification.Recipient,
            Verb = notification.Verb,
            Originator = notification.Originator,
            CorrelationId1 = notification.CorrelationId1,
            CorrelationId2 = notification.CorrelationId2,
        };

        var delivering = "            >>>Delivering>>>        scheduled";

        count = subscribers
            .AsParallel()
            .WithCancellation(_executeCancel.Token)
            .Select((client) =>
            {
                try
                {

                    client.OnNotified(clientMessage);

                    log.LogInformation(
                        $"{delivering} messageId:{notification.MessageId} to subscriber {client.GetHashCode():x8}...");

                    return new DeliveryCount(1, 0);
                }
                catch (Exception ex)
                {
                    log.LogError(
                        @$"{delivering} messageId:{notification.MessageId} to subscriber {client.GetHashCode():x8}...
failure: {ex.Message}
{ex.StackTrace}
");

                    return new DeliveryCount(0, 1);
                }
            })
            .Aggregate((a, b) => a + b);

        updateMessage();

        var delivered = "                         >>>Done>>> scheduled";

        log.LogInformation(
            @$"{delivered} message{(immediate ? "" : "Id:" + notification.MessageId)} to {subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")}...
  {{recipient:{notification.Recipient}, verb:{notification.Verb}, message:{notification.Json.Replace("\"", "")}}}");

        return (count, updates);
    }
}