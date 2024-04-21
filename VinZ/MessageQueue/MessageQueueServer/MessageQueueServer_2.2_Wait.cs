
namespace VinZ.MessageQueue;

public partial class MessageQueueServer
{
    private readonly ConcurrentDictionary<string, AwaitedResponse> _awaitedResponses = new();

    private void Track(AwaitedResponse awaitedResponse)
    {
        if (_awaitedResponses.ContainsKey(awaitedResponse.Key))
        {
            throw new InvalidOperationException("concurrent wait not allowed");
        }

        _awaitedResponses[awaitedResponse.Key] = awaitedResponse;
    }

    private void Untrack(AwaitedResponse awaitedResponse)
    {
        //
        //
        Console.WriteLine($"Correlation={awaitedResponse.Key}, ElapsedSeconds={awaitedResponse.ElapsedSeconds}, Responded={awaitedResponse.Responded}, Cancelled={awaitedResponse.Cancelled}");
        //
        //

        if (!_awaitedResponses.Remove(awaitedResponse.Key, out _))
        {
            throw new InvalidOperationException("invalid wait state");
        }
    }


    private class AwaitedResponse
    (
        ICorrelationId correlationId, ITimeService DateTime, CancellationToken cancellationToken,
        Action<AwaitedResponse> track, Action<AwaitedResponse> untrack
    )
    {
        public string Key { get; } = correlationId.Guid;

        public bool IsCorrelatedTo(ServerNotification notification)
        {
            return
                notification.CorrelationId1 == correlationId.Id1 &&
                notification.CorrelationId2 == correlationId.Id2;
        }

        private DateTime? StartedTime = DateTime.UtcNow;
        public int ElapsedSeconds { get; private set; } = 0;
        public bool Responded { get; private set; }
        public bool Cancelled => cancellationToken.IsCancellationRequested;
        public object? Response { get; private set; }

        public void Respond(object? result)
        {
            if (Responded || Cancelled)
            {
                throw new InvalidOperationException();
            }

            Responded = true;
            Response = result;
        }

        private bool resultAlreadyCalled;

        public object? Result
        {
            get
            {

                if (resultAlreadyCalled)
                {
                    throw new InvalidOperationException();
                }

                resultAlreadyCalled = true;

                track(this);

                var result = WaitAsync().Result;

                untrack(this);

                return result;

            }
        }

        private async Task<object?> WaitAsync()
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

            return Responded;
        }
    }




    public async Task<object?> Wait(INotifyAck ack, CancellationToken cancellationToken)
    {
        var correlationId = ack.CorrelationId;

        if (correlationId == null)
        {
            throw new InvalidOperationException("uncorrelated wait not allowed");
        }

        var awaitedResponse = new AwaitedResponse(correlationId, DateTime, cancellationToken, Track, Untrack);

        return awaitedResponse.Result;

    }

    private void RespondAwaited(ServerNotification notification)
    {
        if (notification.Type != NotificationType.Response)
        {
            return;
        }

        var correlatedWaitedResponses = _awaitedResponses.Values
            .Where(awaitedResponse => awaitedResponse.IsCorrelatedTo(notification))
            .ToArray();

        foreach (var awaitedResponse in correlatedWaitedResponses)
        {
            awaitedResponse.Respond(notification);
        }
    }
}