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

public record ResponseNotification
(
    IClientNotificationSerialized parentNotification,

    string? Recipient,
    string? Verb = Respond,
    object? MessageObj = default,

    TimeSpan? EarliestDelivery = default,
    TimeSpan? LatestDelivery = default,
    TimeSpan? RepeatDelay = default,

    int? RepeatCount = default,
    bool? Aggregate = default,
    bool? Immediate = default,

    bool? Negative = default
)
    : ClientNotification
    (
         [.. (parentNotification?._steps ?? []).Append($"{((Negative ?? false) ? "KO" : "OK")}.{parentNotification?._steps?.LastOrDefault() ?? "null"}")],
        // parentNotification._steps,

        NotificationType.Response,

        Recipient,
        (Negative ?? false) ? (Verb ?? ErrorProcessingRequest) : Verb, 
        MessageObj,

        (int)((Negative??false) ? HttpStatusCode.InternalServerError : HttpStatusCode.OK),
        parentNotification.Originator,

        EarliestDelivery, LatestDelivery, RepeatDelay,
        RepeatCount, Aggregate, Immediate,
        parentNotification.CorrelationId1, parentNotification.CorrelationId2
    ),
        IHaveMessageObj
{
    public ResponseNotification
    (
        IClientNotificationSerialized parentNotification,

        object? MessageObj,

        TimeSpan? EarliestDelivery = default,
        TimeSpan? LatestDelivery = default,
        TimeSpan? RepeatDelay = default,

        int? RepeatCount = default,
        bool? Aggregate = default,
        bool? Immediate = default
    )
        : this
    (
        parentNotification,
        Omni, Respond,
        MessageObj,

        EarliestDelivery, LatestDelivery, RepeatDelay,
        RepeatCount, Aggregate, Immediate
    )
    { }
}