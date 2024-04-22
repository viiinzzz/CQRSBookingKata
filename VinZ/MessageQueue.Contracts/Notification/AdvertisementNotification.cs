using System.Net;

namespace VinZ.MessageQueue;

public record AdvertisementNotification
(
    string? Recipient,
    string? Verb = InformationMessage,

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
        NotificationType.Advertisement,

        Recipient, Verb, Message, (int)HttpStatusCode.Continue,
        EarliestDelivery, LatestDelivery, RepeatDelay,
        CorrelationGuid, RepeatCount, Aggregate, Immediate,
        CorrelationId1, CorrelationId2
    );