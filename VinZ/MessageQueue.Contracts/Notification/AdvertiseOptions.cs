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

public record AdvertiseOptions
(
    string MessageText = null,

    object?[]? Args = default,
    string[]? Steps = default,

    string? Recipient = default,
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
        if (string.IsNullOrEmpty(MessageText))
        {
            throw new ArgumentNullException(nameof(MessageText));
        }
    }


    public static implicit operator ClientNotification(AdvertiseOptions options)
    {
        var _steps = (options.Steps ?? []).AsEnumerable();

        if (!_steps.Any())
        {
            _steps = _steps.Append($"{nameof(Omni)}.{Const.Request}");
        }

        var _recipient = options.Recipient ?? nameof(Omni);
        var _verb = AuditMessage;

        _steps = _steps.Append($"{_recipient}.{_verb}");

        var status = HttpStatusCode.Continue;

        return new ClientNotification
        (
            Steps: [.. _steps],

            Type: NotificationType.Advertisement,

            Recipient: _recipient,
            Verb: _verb,
            messageObj: options.MessageText.Interpolate(options.Args),

            Status: (int)status,
            Originator: options.Originator,

            EarliestDelivery: options.EarliestDelivery, LatestDelivery: options.LatestDelivery,
            RepeatDelay: options.RepeatDelay, RepeatCount: options.RepeatCount,
            Aggregate: options.Aggregate, Immediate: options.Immediate,
            CorrelationId1: options.CorrelationId1, CorrelationId2: options.CorrelationId2
        );
    }

}