namespace VinZ.MessageQueue;

public interface INotifyMessage
{
    string? Recipient { get; }
    string? Verb { get; }
    object? Message { get; }

    TimeSpan? EarliestDelivery { get; }
    TimeSpan? LatestDelivery { get; }
    TimeSpan? RepeatDelay { get; }
    
    int? RepeatCount { get; }
    bool? Aggregate { get; }
    bool? Immediate { get; }

    long? CorrelationId1 { get; }
    long? CorrelationId2 { get; }
}