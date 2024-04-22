namespace VinZ.MessageQueue;

public partial class MqServer
{
    private (DeliveryCount, List<(ServerNotification[], ServerNotificationUpdate)>) Broadcast(ServerNotification notification, bool immediate,
        CancellationToken cancel)
    {
        var updates = new List<(ServerNotification[], ServerNotificationUpdate)>();

        var subscribers = new HashSet<IMessageBusClient>();

        var matchedHash = new List<string>();
        var unmatchedHash = new List<string>();

        // if (notification.Recipient == default && notification.Verb == default)
        {
            var moreSubscribers = _subscribers_0.Values;

            foreach(var subscriber in moreSubscribers)
            {
                subscribers.Add(subscriber);
            }

            if (moreSubscribers.Count > 0)
            {
                matchedHash.Add($"0+");
            }
            else
            {
                unmatchedHash.Add($"0+");
            }
        }
        /*else*/ if (notification.Recipient != default)// && notification.Verb == default)
        {
            var hash_R = notification.Recipient.GetHashCode();

            if (_subscribers_R.TryGetValue(hash_R, out var moreSubscribers))
            {
                foreach (var subscriber in moreSubscribers)
                {
                    subscribers.Add(subscriber);
                }

                matchedHash.Add($"R+{hash_R.xby4()}");
            }
            else
            {
                unmatchedHash.Add($"R+{hash_R.xby4()}");
            }
        }
        /*else*/ if (/*notification.Recipient == default && */notification.Verb != default)
        {
            var hash_V = notification.Verb.GetHashCode();

            if (_subscribers_V.TryGetValue(hash_V, out var moreSubscribers))
            {
                foreach (var subscriber in moreSubscribers)
                {
                    subscribers.Add(subscriber);
                }

                matchedHash.Add($"V+{hash_V.xby4()}");
            }
            else
            {
                unmatchedHash.Add($"V+{hash_V.xby4()}");
            }
        }
        /*else*/ if (notification.Recipient != default && notification.Verb != default)
        {
            var hash_RV = (notification.Recipient, notification.Verb).GetHashCode();

            if (_subscribers_RV.TryGetValue(hash_RV,
                    out var moreSubscribers))
            {
                foreach (var subscriber in moreSubscribers)
                {
                    subscribers.Add(subscriber);
                }

                matchedHash.Add($"RV+{hash_RV.xby4()}");
            }
            else
            {
                unmatchedHash.Add($"RV+{hash_RV.xby4()}");
            }
        }

        var correlationId = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2);

        var dequeuing = immediate ? "            >>>Relaying>>>          immediate" : ">>>Dequeuing>>>                     scheduled";

        log.LogInformation(@$"{dequeuing} message{correlationId} to {subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")}...
  {{recipient:{notification.Recipient}, verb:{notification.Verb
        }, message:{notification.Message.Replace("\"", "")}}}");

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


        //
        //
        var awaitedBus = GetAwaitedBus(notification);

        if (awaitedBus != null)
        {
            subscribers.Add(awaitedBus);
        }
        //
        //


        DeliveryCount count = default;

        if (subscribers.Count == 0)
        {
            if (!immediate) updateMessage();

            log.LogWarning(@$"            >>>Undeliverable!       message{correlationId} ...
  {{recipient:{notification.Recipient}, verb:{notification.Verb}, message:{notification.Message.Replace("\"", "")}}}
  (matched {string.Join(" ", matchedHash)} unmatched {string.Join(" ", unmatchedHash)})");

            return (count, updates);
        }

        var clientMessage = new ClientNotification
        {
            Type = notification.Type,
            Message = notification.Message,
            MessageType = notification.MessageType,
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
            .Select(client =>
            {
                try
                {
                    client.OnNotified(clientMessage);

                    log.LogInformation(
                        $"{delivering} messageId:{notification.MessageId} to subscriber# {client.GetHashCode().xby4()}...");

                    return new DeliveryCount(1, 0);
                }
                catch (Exception ex)
                {
                    log.LogError(
                        @$"{delivering} messageId:{notification.MessageId} to subscriber# {client.GetHashCode().xby4()}...
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
  {{recipient:{notification.Recipient}, verb:{notification.Verb}, message:{notification.Message.Replace("\"", "")}}}");

        return (count, updates);
    }
}