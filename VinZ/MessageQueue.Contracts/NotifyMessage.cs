namespace VinZ.MessageQueue;

public record NotifyMessage(
    string? Recipient = default,
    string? Verb = default,
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
) : INotifyMessage;