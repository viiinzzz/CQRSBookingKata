using System.Globalization;

namespace VinZ.MessageQueue;

public record ClientNotification
(
    string? Message = default,
    string? MessageType = default,
    string? Verb = default,
    string? Recipient = default,
    string? Originator = default,
    long CorrelationId1 = default,
    long CorrelationId2 = default
) 
    : IClientNotification
{
    public TMessage MessageAs<TMessage>()
    {
        if (Message == default)
        {
            throw new ArgumentNullException(nameof(Message));
        }

        if (MessageType == default)
        {
            throw new ArgumentNullException(nameof(MessageType));
        }

        var messageType = Type.GetType(MessageType);

        if (messageType != typeof(TMessage))
        {
            throw new InvalidOperationException($"type mismatch {messageType.FullName} != {typeof(TMessage).FullName}");
        }

        TMessage? messageObj;

        if (typeof(TMessage).IsClass)
        {
            messageObj = JsonConvert.DeserializeObject<TMessage>(Message);

            if (messageObj == null)
            {
                throw new ArgumentNullException(nameof(messageObj));
            }
        }
        else
        {
            messageObj = (TMessage)Convert.ChangeType(Message, typeof(TMessage), CultureInfo.InvariantCulture);
        }

        return messageObj;
    }
}