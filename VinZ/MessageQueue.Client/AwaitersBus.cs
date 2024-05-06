namespace VinZ.MessageQueue;

public class AwaitersBus(AwaitedResponse[] awaitedResponses) : IMessageBusClient
{
    public int SubscribersCount = awaitedResponses.Length;

    // public IMessageBusClient ConnectToBus(IMessageBus bus)
    // {
    //     return this;
    // }

    public ILogger<IMessageBus>? Log { get; set; }

    public IMessageBusClient ConnectToBus(IScopeProvider scp)
    {
        return this;
    }

    public Task Disconnect()
    {
        return Task.CompletedTask;
    }

    public Task Configure()
    {
        return Task.CompletedTask;
    }

    public Task Subscribe(string? recipient, string? verb)
    {
        return Task.CompletedTask;
    }

    public Task<bool> Unsubscribe(string? recipient, string? verb)
    {
        return Task.FromResult(true);
    }

    public Task<NotifyAck> Notify(IClientNotificationSerialized notification)
    {
        return Task.FromResult(new NotifyAck
        {
            Valid = true,
            Status = 0,
            data = default,
            CorrelationId = notification.CorrelationId()
        });
    }

    public void OnNotified(IClientNotificationSerialized notification)
    {
        foreach (var awaitedResponse in awaitedResponses)
        {
            awaitedResponse.Respond(notification);
        }
    }

    public event EventHandler<IClientNotificationSerialized>? Notified;
}