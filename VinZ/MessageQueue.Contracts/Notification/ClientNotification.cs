﻿namespace VinZ.MessageQueue;

public record ClientNotification
(
    NotificationType Type,
    string? Recipient,
    string? Verb,

    int Status = default,

    string? Originator = default,

    TimeSpan? EarliestDelivery = default,
    TimeSpan? LatestDelivery = default,
    TimeSpan? RepeatDelay = default,

    int? RepeatCount = default,
    bool? Aggregate = default,
    bool? Immediate = default,

    long CorrelationId1 = default,
    long CorrelationId2 = default
)
    : IClientNotificationSerialized
{
    public ClientNotification
    (
        NotificationType Type,
        string? Recipient,
        string? Verb,
        object? messageObj,

        int Status = default,

        string? Originator = default,

        TimeSpan? EarliestDelivery = default,
        TimeSpan? LatestDelivery = default,
        TimeSpan? RepeatDelay = default,

        int? RepeatCount = default,
        bool? Aggregate = default,
        bool? Immediate = default,

        long CorrelationId1 = default,
        long CorrelationId2 = default
    )
        : this
    (
        Type,
        Recipient,
        Verb,

        Status,

        Originator,

        EarliestDelivery, LatestDelivery, RepeatDelay,
        RepeatCount, Aggregate, Immediate,

        CorrelationId1, CorrelationId2
    )
    {
        MessageType = messageObj.GetTypeSerializedName();

        Message = messageObj == null ? EmptySerialized : System.Text.Json.JsonSerializer.Serialize(messageObj);
    }


    public string? MessageType { get; }
    public string? Message { get; }


}