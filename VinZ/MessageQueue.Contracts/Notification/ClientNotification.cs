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

public record ClientNotification
(
    NotificationType Type,
    string? Recipient,
    string? Verb,
    int Status = default,
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
    : IClientNotificationSerialized
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string[] _steps { get; set; } = [];
    public int _hops => _steps.Length;

    public ClientNotification
    (
        string[] previousSteps,

        NotificationType Type,
        string? Recipient,
        string? Verb,
        object? messageObj,

        int Status = default,

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
        Type,
        Recipient,
        Verb,
        Status,

        Originator,

        EarliestDelivery: EarliestDelivery, LatestDelivery: LatestDelivery, RepeatDelay: RepeatDelay,
        RepeatCount: RepeatCount, Aggregate: Aggregate, Immediate: Immediate,

        CorrelationId1: CorrelationId1, CorrelationId2: CorrelationId2
    )
    {
        // _steps = [..previousSteps.Append($"{Recipient ?? nameof(Omni)}.{Verb}")];
        _steps = previousSteps ?? [];

        MessageType = messageObj.GetTypeSerializedName();

        if (messageObj?.GetType()?.IsInterface ?? false)
        {
            throw new ArgumentException($"invalid type {messageObj.GetType().FullName} : must be concrete", nameof(messageObj));
        }

        Message = messageObj == null ? EmptySerialized : System.Text.Json.JsonSerializer.Serialize(messageObj);
    }

    public string _type { get; } = "ClientNotification";
    public string? MessageType { get; set;  }
    public string? Message { get; set;  }

}