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

using System.Text.Json.Serialization;

namespace VinZ.MessageQueue;

public record NotifyAck
(
    long correlationId1 = 0,
    long correlationId2 = 0,

    bool Valid = false,

    HttpStatusCode Status = 0,
    string? data = default
)
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public CorrelationId? CorrelationId =>
        correlationId1 == 0 && correlationId2 == 0 ? null : new CorrelationId(correlationId1, correlationId2);

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public Task<object?> Response { get; set; } = Task.FromResult<object?>(null);

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string[]? _steps { get; set; }

    public int hops => _steps?.Length ?? 0;

    public object? response => Response.Status != TaskStatus.RanToCompletion ? null : Response.Result;
}


public static class NotifyAckHelper
{
    public static NotifyAck Ack(this IClientNotificationSerialized notification)
    {
        return new NotifyAck
        {
            _steps = [.. notification._steps.Append("ack")],
            correlationId1 = notification.CorrelationId1,
            correlationId2 = notification.CorrelationId2,
        };
    }
}