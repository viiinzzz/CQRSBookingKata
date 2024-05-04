namespace VinZ.MessageQueue;

public class AwaitedResponse
(
    CorrelationId correlationId, ITimeService DateTime, CancellationToken cancellationToken,
    Action<AwaitedResponse> track, Action<AwaitedResponse> untrack
)
{
    public string Key { get; } = correlationId.Guid;

    public bool IsCorrelatedTo(IHaveCorrelation notification)
    {
        return
            notification.CorrelationId1 == correlationId.Id1 &&
            notification.CorrelationId2 == correlationId.Id2;
    }

    private DateTime? StartedTime = DateTime.UtcNow;
    public int ElapsedSeconds { get; private set; } = 0;
    public bool Responded { get; private set; }
    public bool Cancelled => cancellationToken.IsCancellationRequested;
    public IClientNotificationSerialized? ResponseNotification { get; private set; }

    public void Respond(IClientNotificationSerialized notification)
    {
        if (Responded || Cancelled)
        {
            throw new InvalidOperationException();
        }

        Responded = true;
        ResponseNotification = notification;
    }

    private bool resultAlreadyCalled;

    public IClientNotificationSerialized? ResultNotification
    {
        get
        {

            if (resultAlreadyCalled)
            {
                throw new InvalidOperationException();
            }

            resultAlreadyCalled = true;

            track(this);

            var notification = WaitResponseNotificationAsync().Result;

            untrack(this);

            return notification;

        }
    }

    private async Task<IClientNotificationSerialized?> WaitResponseNotificationAsync()
    {
        while (true)
        {
            await Task.Delay(100);

            if (Responded || Cancelled)
            {
                break;
            }
        }

        ElapsedSeconds = (
            DateTime.UtcNow - StartedTime.Value
        ).Seconds;

        if (Cancelled || !Responded)
        {
            return null;
        }

        return ResponseNotification;
    }
}