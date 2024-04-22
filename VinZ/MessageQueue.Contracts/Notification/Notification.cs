namespace VinZ.MessageQueue;

public record Notificationx
(
    NotificationType Type,
    string? Recipient,
    string? Verb,

    object? Message = default,

    int Status = default,

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
    : INotification;