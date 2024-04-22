using System.Net;

namespace VinZ.MessageQueue;

public record RequestNotification
(
    string? Recipient,
    string? Verb,

    object? Message = default,

    TimeSpan? EarliestDelivery = default,
    TimeSpan? LatestDelivery = default,
    TimeSpan? RepeatDelay = default,

    string? CorrelationGuid = default,
    int? RepeatCount = default,
    bool? Aggregate = default,
    bool? Immediate = default,

    long? CorrelationId1 = default,
    long? CorrelationId2 = default
)
    : Notificationx
    (
        NotificationType.Request,

        Recipient, Verb, Message, 0,
        EarliestDelivery, LatestDelivery, RepeatDelay,
        CorrelationGuid, RepeatCount, Aggregate, Immediate,
        CorrelationId1, CorrelationId2
    );