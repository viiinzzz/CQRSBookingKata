namespace VinZ.MessageQueue;

public static class IMessageBusHelper
{
    public static TReturn? AskResult<TReturn>(
        this IMessageBus mq,
        string originator, string recipient, string requestVerb, object? message
    )
    {
        var task = Ask(mq, originator, recipient, requestVerb, message, CancellationToken.None, ResponseTimeoutSeconds);

        var ret = task.Result;

        return (TReturn?)ret;
    }



    public const string Application = nameof(Application);

    public const int ResponseTimeoutSeconds = 2 * 60;

    public static async Task<TReturn?> Ask<TReturn>(this IMessageBus mq,
        string recipient, string requestVerb,
        string originator, object? message,
        CancellationToken requestCancel, int responseTimeoutSeconds = ResponseTimeoutSeconds)
    {
        var ret = await Ask(mq, originator,  recipient, requestVerb, message, requestCancel, responseTimeoutSeconds);

        return (TReturn?)ret;
    }

    public static async Task<object?> Ask(
        this IMessageBus mq,
        string originator, string recipient, string requestVerb, object? message,
        CancellationToken requestCancel, int responseTimeoutSeconds = ResponseTimeoutSeconds
    )
    {
        var responseTimeoutMilliSeconds = responseTimeoutSeconds * 1000;
        // var responseTimeoutMilliSeconds = 5000; //TODO test override -- remove

        var responseWait = new CancellationTokenSource(responseTimeoutMilliSeconds).Token;

        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(responseWait, requestCancel).Token;

        var requestAck = mq.Notify(Application, new RequestNotification(recipient, requestVerb)
            {
                Message = message
            }
        );

        var ret = await mq.Wait(requestAck, cancellationToken);

        return ret;
    }
}