namespace VinZ.MessageQueue;

public static class MessageBusHelper
{
    public const string Application = nameof(Application);

    public const int ResponseTimeoutSeconds = 2 * 60;

    public static async Task<TReturn> Ask<TReturn>(
        this IMessageBus mq, 
        string recipient, string requestVerb, object? message, string respondVerb,
        CancellationToken requestCancel, int responseTimeoutSeconds = ResponseTimeoutSeconds
    )
    {
        var responseWait = new CancellationTokenSource(responseTimeoutSeconds * 1000).Token;

        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(responseWait, requestCancel).Token;

        var requestAck = mq.Notify(Application, new NotifyMessage(recipient, requestVerb)
            {
                Message = message
            }
        );

        var ret = await mq.Wait<TReturn>(requestAck, recipient, respondVerb, cancellationToken);

        return ret;
    }
}