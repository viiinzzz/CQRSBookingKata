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

public record RequestOptions
(
    string[]? StepsArr = default,
    string? Recipient = default,
    string? Verb = default,
    string? Originator = default,
    object? MessageObj = default,

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
        if (string.IsNullOrEmpty(Recipient))
        {
            throw new ArgumentNullException(nameof(Recipient));
        }
        if (string.IsNullOrEmpty(Verb))
        {
            throw new ArgumentNullException(nameof(Verb));
        } 
        if (StepsArr == null)
        {
            throw new ArgumentNullException(nameof(StepsArr));
        }
    }


    public static implicit operator ClientNotification(RequestOptions options)
    {
        var _steps = (options.StepsArr ?? []).AsEnumerable();

        if (!_steps.Any())
        {
            _steps = _steps.Append($"{nameof(Omni)}.{Const.Request}");
        }

        var _recipient = options.Recipient ?? nameof(Omni);
        var _verb = options.Verb ?? Const.Request;

        _steps = _steps.Append($"{_recipient}.{_verb}");

        // var originator = _steps.First();
        // originator = originator.IndexOf('.') > 0 ? originator[..originator.IndexOf('.')] : originator;

        return new ClientNotification
        (
            Steps: [.. _steps],

            Type: NotificationType.Request,

            Recipient: _recipient,
            Verb: _verb,
            messageObj: options.MessageObj,

            Status: 0,
            Originator: options.Originator ?? nameof(Omni),

            EarliestDelivery: options?.EarliestDelivery, LatestDelivery: options?.LatestDelivery,
            RepeatDelay: options?.RepeatDelay, RepeatCount: options?.RepeatCount,
            Aggregate: options?.Aggregate, Immediate: options?.Immediate,
            CorrelationId1: options?.CorrelationId1 ?? default, CorrelationId2: options?.CorrelationId2 ?? default
        );
    }

}