using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace VinZ.MessageQueue;

public partial class MqServer
{
    private static Regex busIdRx = new(@"^/bus/([^/]+)/$");

    private (DeliveryCount, List<(ServerNotification[], ServerNotificationUpdate)>) Respond(ServerNotification serverNotification, bool immediate,
        CancellationToken cancel)
    {
        var updates = new List<(ServerNotification[], ServerNotificationUpdate)>();

        var subscriberUrls = new HashSet<string>();

        var matchedHash = new List<string>();
        var unmatchedHash = new List<string>();

        // if (notification.Recipient == default && notification.Verb == default)
        {
            var moreSubscriberUrls = _subscribers_0.Values;

            foreach(var subscriberUrl in moreSubscriberUrls)
            {
                subscriberUrls.Add(subscriberUrl);
            }

            if (moreSubscriberUrls.Count > 0)
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

            if (_subscribers_R.TryGetValue(hash_R, out var moreSubscriberUrls))
            {
                foreach (var subscriberUrl in moreSubscriberUrls)
                {
                    subscriberUrls.Add(subscriberUrl);
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

            if (_subscribers_V.TryGetValue(hash_V, out var moreSubscriberUrls))
            {
                foreach (var subscriberUrl in moreSubscriberUrls)
                {
                    subscriberUrls.Add(subscriberUrl);
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

            if (_subscribers_RV.TryGetValue(hash_RV, out var moreSubscriberUrls))
            {
                foreach (var subscriberUrl in moreSubscriberUrls)
                {
                    subscriberUrls.Add(subscriberUrl);
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
        var subscribersCount = $"{subscriberUrls.Count} subscriber{(subscriberUrls.Count > 1 ? "s" : "")}";
        var messageObj = serverNotification.MessageAsObject();
        var messageJson = messageObj.ToJson(true)
            .Replace("\\r", "")
            .Replace("\\n", Environment.NewLine);
        var messageType =
            serverNotification.Type == NotificationType.Response ? "Re: "
            : serverNotification.Type == NotificationType.Advertisement ? "Ad: "
            : "";
        var rvm = @$"---
To: {serverNotification.Recipient}
From: {serverNotification.Originator}
Subject: {messageType}{serverNotification.Verb}
{messageJson}
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
        //
        // if (awaitedBus != null)
        // {
        //     RefreshFastest();
        //
        //     subscriberUrls.Add(awaitedBus);
        // }
        //
        //

        DeliveryCount count = default;

        if (subscriberUrls.Count == 0 && awaitedBus == null)
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

        if (awaitedBus != null)
        {
            RefreshFastest();

            try
            {
                var awaitedAck = awaitedBus.Notify(clientNotification);
                    
                awaitedAck.Wait(_executeCancel.Token);


                // awaitedBus.OnNotified(clientNotification);
                
                log.Log(LogLevel.Debug,
                    $"{delivering} {notificationLabel} to <<<awaitedBus>>>...");

                count += new DeliveryCount(1, 0);
            }
            catch (Exception ex)
            {
                log.LogError(
                    @$"{delivering} {notificationLabel} to <<<awaitedBus>>>...
failure: {ex.Message}
{ex.StackTrace}
");

                count += new DeliveryCount(0, 1);
            }
        }

        if (subscriberUrls.Any()) count += subscriberUrls
            .AsParallel()
            .WithCancellation(_executeCancel.Token)
            .Select(clientUrl =>
            {
                try
                {
                   
                    NotifyAck? ack;

                    var url = new Uri(clientUrl);
                    var busIdMatch = busIdRx.Match(url.PathAndQuery);
                    var busIdStr = !busIdMatch.Success ? string.Empty : busIdMatch.Groups[1].Value;

                    var busIdParsed = ParsableHexInt.TryParse(busIdStr, null, out var busId);

                    if (url.IsLoopback && busIdParsed)
                    {
                        if (!busIdParsed)
                        {
                            return new DeliveryCount(0, 1);
                        }

                        var clientHashCode = busId.Value;

                        var client = _domainBuses.Values
                            .FirstOrDefault(client => client.GetHashCode() == clientHashCode);

                        if (client == null)
                        {
                            return new DeliveryCount(0, 1);
                        }

                        client.OnNotified(clientNotification);

                        ack = new (HttpStatusCode.Accepted, true, null, correlationId);
                    }
                    else
                    {
                        ack = send(clientUrl, clientNotification);
                    }

                    log.Log(LogLevel.Debug,
                        $"{delivering} {notificationLabel} to <<<Subscriber:{clientUrl.GetHashCode().xby4()}>>>...");

                    if (!ack.Valid)
                    {
                        throw new InvalidOperationException($"send failure: ({(int)ack.Status}) {ack.data}");
                    }

                    return new DeliveryCount(1, 0);
                }
                catch (Exception ex)
                {
                    log.LogError(
                        @$"{delivering} {notificationLabel} to <<<Subscriber:{clientUrl.GetHashCode().xby4()}>>>...
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


    private NotifyAck send(string clientUrl, ClientNotification notification)
    {
        var baseAddress = new Uri(clientUrl);
        var uri = nameof(Notify).ToLower();
        var url = baseAddress + uri;

        try
        {
            var cancel = new CancellationTokenSource();

            var json = notification.ToJson();

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var http = new HttpClient
            {
                BaseAddress = baseAddress
            };


            Console.WriteLine(@$"
<<<...........................................................
| HTTP POST {url} (0)
+.............................................................
| {notification.ToJson(true)}
");

            var post = http.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            Console.WriteLine(@$"
                        +.........................................................
                        | HTTP POST {url}
                        +............................................( {(int)post.Result.StatusCode:000} )...>>>
");

            var res = post.Result;

            var statusCode = (int)res.StatusCode;

            if (statusCode is < 200 or > 299)
            {
                return new NotifyAck
                {
                    Valid = false,
                    Status = res.StatusCode,
                    data = @$"failed to reach {url}
{res.ReasonPhrase}"
                };
            }

            var read = res.Content.ReadAsStringAsync(cancel.Token);

            read.Wait(cancel.Token);

            var ack = JsonConvert.DeserializeObject<NotifyAck>(read.Result);

            if (ack == null)
            {
                throw new NullReferenceException(nameof(ack));
            }

            return ack;
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            if (ex is TaskCanceledException cancelEx)
            {
                Console.WriteLine(@$"
                        +.........................................................
                        | HTTP POST {url} ({(int)HttpStatusCode.RequestTimeout})
                        +......................................................>>>
");

                return new NotifyAck
                {
                    Valid = false,
                    Status = HttpStatusCode.RequestTimeout,
                    data = @$"failed to reach {url}"
                };
            }

            Console.WriteLine(@$"
                        +.........................................................
                        | HTTP POST {url} ({(int)HttpStatusCode.InternalServerError})
                        +......................................................>>>
");

            return new NotifyAck
            {
                Valid = false,
                Status = HttpStatusCode.InternalServerError,
                data = @$"failed to reach {url}

---sensitive
{ex.Message}
{ex.StackTrace}"
            };
        }

    }
}