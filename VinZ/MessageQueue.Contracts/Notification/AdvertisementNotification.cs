﻿namespace VinZ.MessageQueue;

public record AdvertisementNotification
(
    string? Recipient,

    object? MessageObj = default,
    
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
    : ClientNotification
    (
        NotificationType.Advertisement,

        Recipient,
        AuditMessage, MessageObj, 
        (int)HttpStatusCode.Continue, Originator,

        EarliestDelivery, LatestDelivery, RepeatDelay,
        RepeatCount, Aggregate, Immediate,
        CorrelationId1, CorrelationId2
    ),
        IHaveMessageObj
{
    public AdvertisementNotification
    (
        string MessageText,
        object?[]? args = default,

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
            Omni,
            MessageObj: Format(MessageText, args),

            Originator,

            EarliestDelivery, LatestDelivery, RepeatDelay,
            RepeatCount, Aggregate, Immediate,
            CorrelationId1, CorrelationId2
        )
    { }

    private static string Format(string messageText, object?[]? args)
    {
        try
        {
            return string.Format(messageText, args ?? []);
        }
        catch
        {
            return $"{messageText} {args.ToJson()}";
        }
    } 
}