using Newtonsoft.Json.Linq;
using VinZ.Common;

namespace VinZ.MessageQueue;


public record NegativeResponseNotification
(
    string? Recipient,

    string? Verb = ErrorProcessingRequest,
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
        NotificationType.Response,

        Recipient, 
        Verb, MessageObj, 
        (int)HttpStatusCode.InternalServerError, Originator,

        EarliestDelivery, LatestDelivery, RepeatDelay,
        RepeatCount, Aggregate, Immediate,
        CorrelationId1, CorrelationId2
    )
{
    public NegativeResponseNotification
    (
        IClientNotificationSerialized childNotification,
        Exception ex,

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
            Omni, ErrorProcessingRequest,

            childNotification
                .MessageAsObject()
                .PatchRelax(new {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }),

            Originator,

            EarliestDelivery, LatestDelivery, RepeatDelay,
            RepeatCount, Aggregate, Immediate,
            CorrelationId1, CorrelationId2
        )
    { }
}