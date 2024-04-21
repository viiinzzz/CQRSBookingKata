namespace VinZ.MessageQueue;

public record ServerNotification
(
    NotificationType Type = NotificationType.Request,

    string? Message = default,
    string? MessageType = default,
    string? Verb = default,
    string? Recipient = default,

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
    int MessageId = default
)
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

        var messageType = System.Type.GetType(MessageType);

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