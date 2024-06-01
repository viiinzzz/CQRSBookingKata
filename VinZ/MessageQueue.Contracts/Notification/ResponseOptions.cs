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

namespace VinZ.MessageQueue;

public record ResponseOptions
(
    IClientNotificationSerialized? ParentNotification = default,

    string? Recipient = default,
    string? Verb = Respond,
    object? MessageObj = default,
    Exception? ex = default,
    // bool Negative = false,
    string? Originator = default,
    
    TimeSpan? EarliestDelivery = default,
    TimeSpan? LatestDelivery = default,

    TimeSpan? RepeatDelay = default,
    int? RepeatCount = default,

    bool? Aggregate = default,
    bool? Immediate = default,

    long CorrelationId1 = default,
    long CorrelationId2 = default
)
    : NotifyOptions
    (
        EarliestDelivery, LatestDelivery, RepeatDelay, RepeatCount, Aggregate, Immediate, CorrelationId1, CorrelationId2
    )
{
    protected new virtual void Validate()
    {
        if (!(MessageObj != null ^ ex != null))
        {
            throw new ArgumentException($"Either set {nameof(MessageObj)} or {nameof(ex)}");
        }
    }

    public static implicit operator ClientNotification(ResponseOptions options)
    {
        if (options.ParentNotification == null)
        {
            throw new ArgumentNullException(nameof(options.ParentNotification));
        }

        if (options != null &&
            (options.CorrelationId1 != 0 || 
             options.CorrelationId2 != 0) && 
            (options.CorrelationId1 != options.ParentNotification.CorrelationId1 ||
             options.CorrelationId2 != options.ParentNotification.CorrelationId2))
        {
            throw new ArgumentException("Mismatch", nameof(CorrelationId));
        }

        var steps = options.ParentNotification._steps ?? [];

        var lastStep = steps.LastOrDefault() ?? nameof(Respond);

        var _steps = steps.Append($"OK.{lastStep}");

        var _recipient = options.Recipient ?? options.ParentNotification.Originator;

        var _verb = options.Verb;

        var _status = HttpStatusCode.OK;

        if (options.ex != null)
        // if (options.Negative)
        {
            _steps = steps.Append($"KO.{lastStep}");
            _verb ??= ErrorProcessingRequest;
            _status = HttpStatusCode.InternalServerError;
        }

        return new ClientNotification
        (
            Steps: [.. _steps],

            Type: NotificationType.Response,

            Recipient: _recipient,
            Verb: _verb,
            messageObj: options.MessageObj,

            Status: (int)_status,
            Originator: options.ParentNotification.Originator,

            EarliestDelivery: options?.EarliestDelivery, LatestDelivery: options?.LatestDelivery,
            RepeatDelay: options?.RepeatDelay, RepeatCount: options?.RepeatCount,
            Aggregate: options?.Aggregate, Immediate: options?.Immediate,

            CorrelationId1: options.ParentNotification.CorrelationId1, CorrelationId2: options.ParentNotification.CorrelationId2
        );
    }
}



public static class ClientNotificationHelper
{
    public static ClientNotification Response(this IClientNotificationSerialized parentNotification, ResponseOptions options)
    {
        return options with
        {
            ParentNotification = parentNotification
        };
    }

    public static ClientNotification Response(this IClientNotificationSerialized parentNotification, Exception ex)
    {
        return new ResponseOptions
        {
            ParentNotification = parentNotification,
            ex = ex
        };
    }
    public static ClientNotification Response(this IClientNotificationSerialized parentNotification, object messageObj)
    {
        return new ResponseOptions
        {
            ParentNotification = parentNotification,
            MessageObj = messageObj
        };
    }
}