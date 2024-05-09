using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Transactions;

namespace VinZ.MessageQueue;

public static class IMessageBusHelper
{
    public static TReturn? AskResult<TReturn>
    (
        this IMessageBus mq,
        string recipient, string requestVerb, object? message,
        string originator
    )
        where TReturn : class
    {
        var task = Ask<TReturn>(mq,
            originator, recipient, requestVerb, message,
            CancellationToken.None, ResponseTimeoutSeconds);

        task.Wait();

        var ret = task.Result;

        return ret;
    }



    public const string Application = nameof(Application);

    public const int ResponseTimeoutSeconds = 2 * 60;

    public static async Task<ExpandoObject> AskObject(
        this IMessageBus mq,
        string originator, string recipient, string requestVerb, object? message,
        CancellationToken requestCancel, int responseTimeoutSeconds = ResponseTimeoutSeconds
    )
    {
        return await mq.Ask<ExpandoObject>(
            originator, recipient, requestVerb, message,
            requestCancel, responseTimeoutSeconds) 
               ?? new ExpandoObject();
    }

    public static async Task<TReturn?> Ask<TReturn>(
        this IMessageBus mq,
        string originator, string recipient, string requestVerb, object? message,
        CancellationToken requestCancel, int responseTimeoutSeconds = ResponseTimeoutSeconds
    )
        where TReturn : class
    {
        var responseTimeoutMilliSeconds = responseTimeoutSeconds * 1000;
        // var responseTimeoutMilliSeconds = 5000; //TODO test override -- remove

        var responseWait = new CancellationTokenSource(responseTimeoutMilliSeconds).Token;

        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(responseWait, requestCancel).Token;

        var requestAck = mq.Notify(new RequestNotification(recipient, requestVerb,  message)
        {
            Immediate = true
        }, 0);

        var notification = await mq.Wait(requestAck, cancellationToken);

        if (typeof(TReturn) == typeof(ExpandoObject))
        {
            var messageObj = JsonConvert.DeserializeObject<ExpandoObject>(notification.Message, new ExpandoObjectConverter());

            return messageObj as TReturn;
        }

        if (notification.Verb == ErrorProcessingRequest)
        {
            // var messageObj = notification.MessageAsObject();
            //
            // throw new AskException(ErrorProcessingRequest, messageObj);

            return null;
        }

        var ret = notification.MessageAs<TReturn>();

        return ret;
    }
}

// public class AskException(string verb, object? messageObj) : Exception(verb)
// {
//
// } 