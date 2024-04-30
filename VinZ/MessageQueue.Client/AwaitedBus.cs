namespace VinZ.MessageQueue;

public class AwaitedBus(AwaitedResponse[] awaitedResponses) : IMessageBusClient
{
    // public IMessageBusClient ConnectToBus(IMessageBus bus)
    // {
    //     return this;
    // }

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

    public Task Notify(IClientNotificationSerialized message)
    {
        return Task.CompletedTask;
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