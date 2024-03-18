namespace Vinz.MessageQueue;

public record NotifyMessage(
    string? Recipient = default,
    string? Verb = default,
    object? Message = default,

    TimeSpan? EarliestDelivery = default,
    TimeSpan? LatestDelivery = default,
    TimeSpan? RepeatDelay = default,
    int? RepeatCount = default,
    bool? Aggregate = default,
    bool? Immediate = default
) : INotifyMessage;