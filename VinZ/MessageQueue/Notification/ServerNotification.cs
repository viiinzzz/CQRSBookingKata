﻿/*
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

public record ServerNotification
(
    string? History = default,

    NotificationType Type = NotificationType.Request,

    string? Recipient = default,
    string? Verb = default,
    string? Message = default,
    string? MessageType = default,

    int Status = default,

    DateTime NotificationTime = default,
    DateTime EarliestDelivery = default,
    DateTime LatestDelivery = default,

    TimeSpan RepeatDelay = default,
    bool Aggregate = default,

    int RepeatCount = default,
    bool Done = false,
    DateTime DoneTime = default,

    string? Originator = default,
    long CorrelationId1 = default,
    long CorrelationId2 = default,
    int NotificationId = default
)
   : IHaveSerializedMessage, IHaveDeliveryStatus, IHaveCorrelation
{
    public string[] HistorySteps => StepsFromHistory(History);
    public static string[] StepsFromHistory(string? history) => (history ?? string.Empty).Split(',');
    public static string HistoryFromSteps(string[]? steps) => string.Join(',', steps ?? []);
}
