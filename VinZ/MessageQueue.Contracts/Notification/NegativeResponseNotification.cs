using System.Dynamic;

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
        bool? Immediate = default
    )
        : this
        (
            Omni, ErrorProcessingRequest,

            
            childNotification.MessageAsObject().IncludeError(ex),

            childNotification.Originator,

            EarliestDelivery, LatestDelivery, RepeatDelay,
            RepeatCount, Aggregate, Immediate,
            childNotification.CorrelationId1, childNotification.CorrelationId2
        )
    { }


    public NegativeResponseNotification
    (
        string recipient,
        IClientNotificationSerialized childNotification,
        Exception ex,

        string? Originator = default,

        TimeSpan? EarliestDelivery = default,
        TimeSpan? LatestDelivery = default,
        TimeSpan? RepeatDelay = default,

        int? RepeatCount = default,
        bool? Aggregate = default,
        bool? Immediate = default
    )
        : this
        (
            recipient, ErrorProcessingRequest,

            childNotification.MessageAsObject().IncludeError(ex),

            childNotification.Originator,

            EarliestDelivery, LatestDelivery, RepeatDelay,
            RepeatCount, Aggregate, Immediate,
            childNotification.CorrelationId1, childNotification.CorrelationId2
        )
    { }
}


public static class NegativeResponseNotificationHelper
{

    public static ExpandoObject? IncludeError(this object obj, Exception ex)
    {
        var ret = obj.PatchRelax(new
        {
            error = ex.Message,
            stackTrace = $"{Environment.NewLine}{ex.StackTrace}"
        });

        if (ex.InnerException != null)
        {
            ret = ret.PatchRelax(new
            {
                errorInner = IncludeError(null, ex.InnerException)
            });
        }

        return ret;
    }
}