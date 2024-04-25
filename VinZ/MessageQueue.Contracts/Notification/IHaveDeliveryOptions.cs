namespace VinZ.MessageQueue;

public interface IHaveDeliveryOptions
    : IHaveDeliveryStatus
{
    NotificationType Type { get; }

    TimeSpan? EarliestDelivery { get; }
    TimeSpan? LatestDelivery { get; }
    TimeSpan? RepeatDelay { get; }

    int? RepeatCount { get; }
    bool? Aggregate { get; }
    bool? Immediate { get; }
}