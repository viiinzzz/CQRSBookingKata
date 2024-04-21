namespace VinZ.MessageQueue;

public record ResponseNotification
(
    string? Recipient,
    string? Verb = Respond,

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
        NotificationType.Response,

        Recipient, Verb, Message,
        EarliestDelivery, LatestDelivery, RepeatDelay,
        CorrelationGuid, RepeatCount, Aggregate, Immediate,
        CorrelationId1, CorrelationId2
    );