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
    public NotifyAck Notify(IClientNotificationSerialized notification, int busId)
    {
        var bus = _domainBuses.Values.FirstOrDefault(bus => bus.GetHashCode() == busId);

        if (bus != null)
        {
            return bus.Notify(notification).Result;
        }

        return Notify(notification, CancellationToken.None);
    }

    public NotifyAck Notify(IClientNotificationSerialized clientNotification, CancellationToken cancel)
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
            Steps = n._steps,

            NotificationTime = now,
            EarliestDelivery = selectEarliest ? new[] { now2, now + n.EarliestDelivery.Value }.Max() : now,
            LatestDelivery = selectLatest ? now + n.LatestDelivery.Value : System.DateTime.MaxValue,
            RepeatDelay = n.RepeatDelay ?? TimeSpan.Zero,
            RepeatCount = !selectRepeat ? 0 : immediate ? n.RepeatCount.Value - 1 : n.RepeatCount.Value,
            Aggregate = n.Aggregate ?? false,
        };

        string[] steps = [.. (notification.Steps ?? []).Append($"{notification.Recipient ?? nameof(Omni)}.{notification.Verb}")];


        var ack = new NotifyAck
        {
            Valid = true,
            _steps = steps,
            correlationId1 = correlationId.Id1,
            correlationId2 = correlationId.Id2
        };

        if (immediate)
        {
            //
            //
            var (count, updates) = Respond(notification, immediate: true, cancel);
            //
            //

            if (count.Delivered > 0)
            {
                RefreshFastest();
            }

            return ack;
        }

        if (!immediate || notification.RepeatCount > 0)
        {
            using var scope = scp.GetScope<IMessageQueueRepository>(out var queue);


            var queuing = immediate ? "                     <<<Relaying<<< Immediate" : "                      <<<Queuing<<< Scheduled";
            var notificationLabel = $"Notification{correlationId.Guid}";
            var messageObj = notification.MessageAsObject();
            var messageJson = messageObj.ToJson(true)
                .Replace("\\r", "")
                .Replace("\\n", Environment.NewLine);
            var messageType = 
                notification.Type == NotificationType.Response ? "Re: " 
                : notification.Type == NotificationType.Advertisement ? "Ad: " 
                : "";
            var rvm = @$"---
To: {Href(notification.Recipient ?? nameof(Omni))}
From: {notification.Originator}
Subject: {Bold}{messageType}{Fg(notification.Verb == ErrorProcessingRequest ? Color.Red : Color.PaleGreen)}{notification.Verb}{Rs}
{string.Join('\n', messageJson.Split('\n').Select(line => $"{Faint}{line}{Rs}"))}
---";

            {
                var message = @$"{queuing} {notificationLabel}...{Environment.NewLine}{rvm}";

                LogLevel? logLevel = 
                    notification.IsErrorStatus() ? LogLevel.Warning
                    : _isTrace ? LogLevel.Information
                    : null;

                if (logLevel.HasValue)
                {
                    log.Log(logLevel.Value, message);
                }

                Check(logLevel ?? LogLevel.Debug);
            }
            

            //
            //
            queue.AddNotification(notification);
            //
            //

            return ack;
        }

        return new NotifyAck
        {
            Valid = false,
            _steps = steps,
            correlationId1 = correlationId.Id1,
            correlationId2 = correlationId.Id2
        };
    }
}