using System.Net;

namespace VinZ.MessageQueue;

public partial class MqServer
{
    private (DeliveryCount, List<(ServerNotification[], ServerNotificationUpdate)>) Respond(ServerNotification serverNotification, bool immediate,
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
        /*else*/ if (serverNotification.Recipient != default)// && notification.Verb == default)
        {
            var hash_R = serverNotification.Recipient.GetHashCode();

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
        /*else*/ if (/*notification.Recipient == default && */serverNotification.Verb != default)
        {
            var hash_V = serverNotification.Verb.GetHashCode();

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
        /*else*/ if (serverNotification.Recipient != default && serverNotification.Verb != default)
        {
            var hash_RV = (serverNotification.Recipient, serverNotification.Verb).GetHashCode();

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

        var correlationId = new CorrelationId(serverNotification.CorrelationId1, serverNotification.CorrelationId2);
        var notificationLabel = $"Notification{correlationId}{(immediate ? "" : $" (Id:{serverNotification.NotificationId})")}";
        var dequeuing = immediate ? "            >>>Relaying>>>          Immediate" : ">>>Dequeuing>>>                     scheduled";
        var subscribersCount = $"{subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")}";
        var messageObj = serverNotification.MessageAsObject();
        var messageObjString = JsonConvert.SerializeObject(messageObj, Formatting.Indented).Replace("\\r", "").Replace("\\n", Environment.NewLine);
        var messageType =
            serverNotification.Type == NotificationType.Response ? "Re: "
            : serverNotification.Type == NotificationType.Advertisement ? "Ad: "
            : "";
        var rvm = @$"---
To: {serverNotification.Recipient}
From: {serverNotification.Originator}
Subject: {messageType}{serverNotification.Verb}
{messageObjString}
---";
        {
            var logLevel =
                serverNotification.IsErrorStatus() ? LogLevel.Error
                // : serverNotification.Verb == AuditMessage ? LogLevel.Debug
                : LogLevel.Debug;

            log.Log(logLevel,
                @$"{dequeuing} {notificationLabel} to {subscribersCount}...{Environment.NewLine}{rvm}");

            Check(logLevel);
        }

        var updateMessage = () =>
        {
            if (serverNotification is { RepeatDelay: { Ticks: > 0 }, RepeatCount: > 1 })
            {
                updates.Add((new[] { serverNotification }, new ServerNotificationUpdate
                {
                    RepeatCount = serverNotification.RepeatCount - 1
                }));
            }
            else
            {
                updates.Add((new[] { serverNotification }, new ServerNotificationUpdate
                {
                    Done = true,
                    DoneTime = DateTime.UtcNow
                }));
            }
        };


        //
        //
        var awaitedBus = GetAwaitedBus(serverNotification);

        if (awaitedBus != null)
        {
            RefreshFastest();

            subscribers.Add(awaitedBus);
        }
        //
        //


        DeliveryCount count = default;

        if (subscribers.Count == 0)
        {
            if (!immediate) updateMessage();

            var matchedUnmatched =
                $"(matched {string.Join(" ", matchedHash)} unmatched {string.Join(" ", unmatchedHash)})";
            {
                var logLevel =
                    serverNotification.Verb == ErrorProcessingRequest
                        ? LogLevel.Debug
                        : LogLevel.Error;

                log.Log(logLevel,
                    @$"            >>>Undeliverable!       {notificationLabel}...{Environment.NewLine}{matchedUnmatched}{Environment.NewLine}{rvm}");

                Check(logLevel);
            }

            if (serverNotification.IsErrorStatus())
            {
                //if a liable message (not an error report),
                //inform originator message got lost 424 (unavailable fringe bus?)

                var nack = new NegativeResponseNotification(serverNotification.Originator)
                {
                    MessageObj = new
                    {
                        reason = "Undeliverable"
                    },

                    Status = (int)HttpStatusCode.FailedDependency,

                    CorrelationId1 = serverNotification.CorrelationId1,
                    CorrelationId2 = serverNotification.CorrelationId2,
                };

            }

            return (count, updates);
        }

        var clientNotification = new ClientNotification
        (
            serverNotification.Type, 
            serverNotification.Recipient, serverNotification.Verb, 
            serverNotification.MessageAsObject())
        {
            // Originator = serverNotification.Originator,
            CorrelationId1 = serverNotification.CorrelationId1,
            CorrelationId2 = serverNotification.CorrelationId2,
        };

        var delivering = "            >>>Delivering>>>        scheduled";



        count = subscribers
            .AsParallel()
            .WithCancellation(_executeCancel.Token)
            .Select(client =>
            {
                try
                {
                    client.OnNotified(clientNotification);

                    log.Log(LogLevel.Debug,
                        $"{delivering} {notificationLabel} to <<<Subscriber:{client.GetHashCode().xby4()}>>>...");

                    return new DeliveryCount(1, 0);
                }
                catch (Exception ex)
                {
                    log.LogError(
                        @$"{delivering} {notificationLabel} to <<<Subscriber:{client.GetHashCode().xby4()}>>>...
failure: {ex.Message}
{ex.StackTrace}
");

                    return new DeliveryCount(0, 1);
                }
            })
            .Aggregate((a, b) => a + b);

        updateMessage();

        var sent = "                         >>>Sent>>> scheduled";

        {
            var logLevel =
                clientNotification.IsErrorStatus() ? LogLevel.Error
                : clientNotification.Verb == AuditMessage ? LogLevel.Debug
                : LogLevel.Information;

            log.Log(logLevel,
                @$"{sent} {notificationLabel} to {subscribersCount}...{Environment.NewLine}{rvm}");

            Check(logLevel);
        }

        return (count, updates);
    }
}