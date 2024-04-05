namespace VinZ.MessageQueue;

public record ServerNotification
(
    string? Json = default,
    string? Verb = default,
    string? Recipient = default,

    DateTime MessageTime = default,
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
    int MessageId = default
);

public record ServerNotificationUpdate
(
    int? RepeatCount = default,
    bool? Done = default,
    DateTime? DoneTime = default
);