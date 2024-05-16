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

using System.Dynamic;

namespace VinZ.MessageQueue;


public record NegativeResponseNotification
(
    string? Recipient,

    string? Verb = ErrorProcessingRequest,
    
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
        NotificationType.Response,

        Recipient, 
        Verb, MessageObj, 
        (int)HttpStatusCode.InternalServerError, Originator,

        EarliestDelivery, LatestDelivery, RepeatDelay,
        RepeatCount, Aggregate, Immediate,
        CorrelationId1, CorrelationId2
    ),
        IHaveMessageObj
{
    public NegativeResponseNotification
    (
        IClientNotificationSerialized childNotification,
        Exception ex,

        string? Originator = default,

        TimeSpan? EarliestDelivery = default,
        TimeSpan? LatestDelivery = default,
        TimeSpan? RepeatDelay = default,

        int? RepeatCount = default,
        bool? Aggregate = default,
        bool? Immediate = default
    )
        : this
        (
            Omni, ErrorProcessingRequest,

            
            childNotification.MessageAsObject().IncludeError(ex),

            childNotification.Originator,

            EarliestDelivery, LatestDelivery, RepeatDelay,
            RepeatCount, Aggregate, Immediate,
            childNotification.CorrelationId1, childNotification.CorrelationId2
        )
    { }


    public NegativeResponseNotification
    (
        string recipient,
        IClientNotificationSerialized childNotification,
        Exception ex,

        string? Originator = default,

        TimeSpan? EarliestDelivery = default,
        TimeSpan? LatestDelivery = default,
        TimeSpan? RepeatDelay = default,

        int? RepeatCount = default,
        bool? Aggregate = default,
        bool? Immediate = default
    )
        : this
        (
            recipient, ErrorProcessingRequest,

            childNotification.MessageAsObject().IncludeError(ex),

            childNotification.Originator,

            EarliestDelivery, LatestDelivery, RepeatDelay,
            RepeatCount, Aggregate, Immediate,
            childNotification.CorrelationId1, childNotification.CorrelationId2
        )
    { }
}


public static class NegativeResponseNotificationHelper
{

    public static ExpandoObject? IncludeError(this object obj, Exception ex)
    {
        var ret = obj.PatchRelax(new
        {
            _error = ex.Message,
        });

        if (!ex.Message.StartsWith("SQLite Error"))
        {
            var stacktrace = (ex.StackTrace ?? string.Empty)
                .Split(Environment.NewLine)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0 &&
                               !line.StartsWith("at Microsoft.EntityFrameworkCore"))
                .ToArray();

            if (stacktrace.Length > 0)
            {
                ret = ret.PatchRelax(new
                {
                    _errorStackTrace = stacktrace
                });
            }
        }

        if (ex.InnerException != null)
        {
            ret = ret.PatchRelax(new
            {
                errorInner = IncludeError(null, ex.InnerException)
            });
        }

        return ret;
    }
}