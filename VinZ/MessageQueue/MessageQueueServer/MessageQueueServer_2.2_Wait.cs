namespace VinZ.MessageQueue;

public partial class MessageQueueServer
{
    private readonly ConcurrentDictionary<string, object> _waitedResponses = new();

    public async Task<object?> Wait(INotifyAck ack, CancellationToken cancellationToken)
    {
        var key = ack.CorrelationId.Guid;

        if (_waitedResponses.ContainsKey(key))
        {
            throw new InvalidOperationException("concurrent wait not allowed");
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(100);

            if (!_waitedResponses.TryRemove(key, out var waitedResponse))
            {
                continue;
            }

            return waitedResponse;
        }
    }
}