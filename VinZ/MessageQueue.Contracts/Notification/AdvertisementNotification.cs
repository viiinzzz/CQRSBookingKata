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

public record AdvertisementNotification
(
    string? Recipient,

    object? MessageObj = default,
    
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
    : ClientNotification
    (
        NotificationType.Advertisement,

        Recipient,
        AuditMessage, MessageObj, 
        (int)HttpStatusCode.Continue, Originator,

        EarliestDelivery, LatestDelivery, RepeatDelay,
        RepeatCount, Aggregate, Immediate,
        CorrelationId1, CorrelationId2
    ),
        IHaveMessageObj
{
    public AdvertisementNotification
    (
        string MessageText,
        object?[]? args = default,

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
        : this
        (
            Omni,
            MessageObj: Format(MessageText, args),

            Originator,

            EarliestDelivery, LatestDelivery, RepeatDelay,
            RepeatCount, Aggregate, Immediate,
            CorrelationId1, CorrelationId2
        )
    { }

    private static string Format(string messageText, object?[]? args)
    {
        try
        {
            return string.Format(messageText, args ?? []);
        }
        catch
        {
            return $"{messageText} {args.ToJson()}";
        }
    } 
}