namespace Vinz.MessageQueue;

public record ServerMessage
(
    string? Json = default,
    string? Verb = default,
    string? Recipient = default,

    DateTime MessageTime = default,
    DateTime EarliestDelivery = default,
    DateTime LatestDelivery = default,
    TimeSpan RepeatDelay = default,
    int RepeatCount = default,
    bool Aggregate = default,

    int MessageId = 0
);