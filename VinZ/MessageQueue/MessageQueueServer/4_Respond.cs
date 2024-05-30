/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Drawing;

namespace VinZ.MessageQueue;

public partial class MqServer
{
    private static readonly Regex BusIdRx = new(@"^/bus/([^/]+)/$");

    private readonly IPAddress[] _myIps= Dns.GetHostEntry(Dns.GetHostEntry("").HostName).AddressList;

    public record DeliveryCountAndUpdates(
        DeliveryCount count,
        List<(ServerNotification[], ServerNotificationUpdate)> updates)
    {
        public static DeliveryCountAndUpdates operator +(DeliveryCountAndUpdates a, DeliveryCountAndUpdates b)
        {
            return new DeliveryCountAndUpdates
            (
                a.count + b.count,
                a.updates.Concat(b.updates).ToList()
            );
        }
    }

    private DeliveryCountAndUpdates Respond
    (
        ServerNotification serverNotification, 
        bool immediate,
        CancellationToken cancel
    )
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

        //
        //
        var awaitersBus = GetAwaitersBus(serverNotification);
        //
        //


        var correlationId = new CorrelationId(serverNotification.CorrelationId1, serverNotification.CorrelationId2);
        var notificationLabel = $"Notification{correlationId}{(immediate ? "" : $" (Id:{serverNotification.NotificationId})")}";
        var dequeuing = immediate 
            ? $"{Inverted}{Fg(Color.LemonChiffon)}            >>>Relaying>>>         {Rs} Immediate" 
            : $"{Inverted}{Fg(Color.SkyBlue)}>>>Dequeuing>>>                    {Rs} scheduled";
        var subscribersCountStr = $"{subscriberUrls.Count + (awaitersBus?.SubscribersCount ?? 0)} subscriber{(subscriberUrls.Count + (awaitersBus?.SubscribersCount ?? 0) > 1 ? "s" : "")}";
        var messageObj = serverNotification.MessageAsObject();
        var messageHeaders = $"{Faint}---{Rs} {Fg(Color.DarkMagenta)}_steps{Rs} {Faint}={Rs} {Bold}{Fg(Color.DarkMagenta)}{string.Join(", ", serverNotification.Steps ?? [])}{Rs}";
        var messageJson = messageObj.ToJson(true)
            .Replace("\\r", "")
            .Replace("\\n", Environment.NewLine);
        var messageType =
            serverNotification.Type == NotificationType.Response ? "Re: "
            : serverNotification.Type == NotificationType.Advertisement ? "Ad: "
            : "";
        var rvm = @$"---
To: {Href(serverNotification.Recipient ?? nameof(Omni))}
From: {serverNotification.Originator}
Subject: {Bold}{messageType}{serverNotification.Verb}{Rs}
{messageHeaders}
{string.Join('\n', messageJson.Split('\n').Select(line => $"{Faint}{line}{Rs}"))}
---";
        {
            var message =
                @$"{dequeuing} {notificationLabel}
                                    to {subscribersCountStr}...{Environment.NewLine}{rvm}";

            LogLevel? logLevel =
                serverNotification.IsErrorStatus() ? LogLevel.Error
                : _isTrace ? LogLevel.Information
                : null;

            if (logLevel.HasValue)
            {
                log.Log(logLevel.Value, message);
            }

            Check(logLevel ?? LogLevel.Debug);
        }




        var updateRepeatMessage = () =>
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


        DeliveryCount count = default;

        if (subscriberUrls.Count == 0 && awaitersBus == null)
        {
            if (!immediate) updateRepeatMessage();

            {
                var matchedUnmatched =
                    $"(matched {string.Join(" ", matchedHash)} unmatched {string.Join(" ", unmatchedHash)})";

                var message =
                    @$"{Inverted}{Fg(Color.Orange)}                  >>>Undeliverable !{Rs}{notificationLabel}...{Environment.NewLine}{matchedUnmatched}{Environment.NewLine}{rvm}";

                LogLevel? logLevel =
                    serverNotification.Verb != ErrorProcessingRequest ? LogLevel.Error
                    : _isTrace ? LogLevel.Information
                    : null;

                if (logLevel.HasValue)
                {
                    log.Log(logLevel.Value, message);
                }

                Check(logLevel ?? LogLevel.Debug);
            }

            if (serverNotification.IsErrorStatus())
            {
                //if a liable message (not an error report),
                //inform originator message got lost 424 (unavailable fringe bus?)

                var notification = new RequestNotification(serverNotification.Steps ?? [], serverNotification.Recipient, serverNotification.Verb)
                {
                    CorrelationId1 = serverNotification.CorrelationId1,
                    CorrelationId2 = serverNotification.CorrelationId2,
                    Originator = serverNotification.Originator
                };

                var nack = new NegativeResponseNotification(notification, new Exception("Undeliverable"));

            }

            return new DeliveryCountAndUpdates(count, updates);
        }

        var clientNotification = new ClientNotification
        (
            serverNotification.Steps ?? [],
            serverNotification.Type, 
            serverNotification.Recipient, serverNotification.Verb, 
            serverNotification.MessageAsObject())
        {
            // Originator = serverNotification.Originator,
            CorrelationId1 = serverNotification.CorrelationId1,
            CorrelationId2 = serverNotification.CorrelationId2,
        };

        var delivering = $"{Inverted}{Fg(Color.PaleGreen)}            >>>Delivering>>>       {Rs} scheduled";

        if (awaitersBus != null)
        {
            RefreshFastest();

            try
            {
                var awaitedAckTask = awaitersBus.Notify(clientNotification);
                awaitedAckTask.Wait(_executeCancel.Token);
                var awaitedAck = awaitedAckTask.Result;

                awaitersBus.OnNotified(clientNotification);

                if (_isTrace) log.LogInformation(
                    @$"{delivering} {notificationLabel}
                                    to <<<awaitedBus>>>...");

                count += new DeliveryCount(1, 0);
            }
            catch (Exception ex)
            {
                log.LogError(
                    @$"{delivering} {notificationLabel}
                                    to <<<awaitedBus>>>...
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
                    var busIdMatch = BusIdRx.Match(url.PathAndQuery);
                    var busIdStr = !busIdMatch.Success ? string.Empty : busIdMatch.Groups[1].Value;

                    var busIdParsed = ParsableHexInt.TryParse(busIdStr, null, out var busId)
                        && busId.Value != 0;

                    // var isLoopback = url.IsLoopback
                    var isLoopback = Dns.GetHostEntry(url.Host).AddressList
                        .Any(a => _myIps.Contains(a));

                    if (isLoopback)
                    {
                        if (!busIdParsed)
                        {
                            log.LogWarning(
                                $"Invalid busId {0:0} in client url {clientUrl}");

                            return new DeliveryCount(0, 1);
                        }

                        var clientHashCode = busId.Value;

                        var client = _domainBuses.Values
                            .FirstOrDefault(client => client.GetHashCode() == clientHashCode);

                        if (client == null)
                        {
                            log.LogWarning(
                                $"Invalid busId {clientHashCode.xby4()} in client url {clientUrl}");

                            return new DeliveryCount(0, 1);
                        }

                        client.OnNotified(clientNotification);

                        ack = new NotifyAck {
                            Valid = true,
                            // _steps = serverNotification.Steps ?? [],
                            _steps = clientNotification._steps,
                            Status = HttpStatusCode.Accepted,
                            correlationId1 = correlationId.Id1,
                            correlationId2 = correlationId.Id2
                        };
                    }
                    else
                    {
                        ack = HttpSend(clientUrl, clientNotification);
                    }

                    if (_isTrace) log.LogInformation(
                        $"{delivering} {notificationLabel} to <<<Subscriber:{clientUrl.GetHashCode().xby4()}>>>...");

                    if (!ack.Valid)
                    {
                        throw new InvalidOperationException($"send failure: ({(int)ack.Status}) {ack.data}");
                    }

                    RefreshFastest();
                    return new DeliveryCount(1, 0);
                }
                catch (Exception ex)
                {
                    log.LogError(
                        @$"{delivering} {notificationLabel} to <<<Subscriber:{clientUrl.GetHashCode().xby4()}>>>...
failure: {ex.Message}
{ex.StackTrace}
");

                    RefreshFastest();
                    return new DeliveryCount(0, 1);
                }
            })
            .Aggregate((a, b) => a + b);

        updateRepeatMessage();

        {
            LogLevel? logLevel =
                clientNotification.IsErrorStatus() || 
                count.Delivered == 0 || count.Failed > 0 ? LogLevel.Error
                : clientNotification.Verb == AuditMessage || _isTrace ? LogLevel.Information
                : null;

            if (logLevel.HasValue)
            {
                var sent = $"{Inverted}{Fg(count.Failed > 0 || count.Delivered == 0 ? Color.Red : Color.Green)}                         >>>Sent>>>{Rs} scheduled";
                var countStr = $"{subscribersCountStr}, {count.Delivered} delivered, {count.Failed} undelivered";
                var message = @$"{sent} {notificationLabel}
                                    to {countStr}...{Environment.NewLine}{rvm}";
             
                log.Log(logLevel.Value, message);
            }

            Check(logLevel ?? LogLevel.Debug);
        }

        return new DeliveryCountAndUpdates(count, updates);
    }
}