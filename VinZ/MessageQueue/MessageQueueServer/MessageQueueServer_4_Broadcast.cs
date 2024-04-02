namespace VinZ.MessageQueue;

public partial class MessageQueueServer
{
    private async Task<(DeliveryCount, List<(ServerNotification[], ServerNotificationUpdate)>)> Broadcast(ServerNotification notification, bool immediate,
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

        Console.Out.WriteLine($"{dequeuing} message{(immediate ? "" : "Id:" + notification.MessageId)
        } to {subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")
        }... {{recipient:{notification.Recipient}, verb:{notification.Verb
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
        };

        var delivering = "            >>>Delivering>>>        scheduled";

        count = subscribers
            .AsParallel()
            .WithCancellation(_executeCancel.Token)
            .Select((client) =>
            {
                try
                {
                    Console.Out.WriteLine(
                        $"{delivering} messageId:{notification.MessageId} to subscriber {client.GetHashCode():x8}...");

                    client.OnNotified(clientMessage);

                    return new DeliveryCount(1, 0);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());

                    return new DeliveryCount(0, 1);
                }
            })
            .Aggregate((a, b) => a + b);

        updateMessage();

        var delivered = "                         >>>Done>>> scheduled";

        Console.Out.WriteLine(
            $"{delivered} message{(immediate ? "" : "Id:" + notification.MessageId)} to {subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")}... {{recipient:{notification.Recipient}, verb:{notification.Verb}, message:{notification.Json.Replace("\"", "")}}}");

        return (count, updates);
    }
}