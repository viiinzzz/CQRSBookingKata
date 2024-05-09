
namespace VinZ.MessageQueue;

public record ServerNotification
(
    NotificationType Type = NotificationType.Request,

    string? Recipient = default,
    string? Verb = default,
    string? Message = default,
    string? MessageType = default,

    int Status = default,

    DateTime NotificationTime = default,
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
    int NotificationId = default
)
   : IHaveSerializedMessage, IHaveDeliveryStatus, IHaveCorrelation
{
}