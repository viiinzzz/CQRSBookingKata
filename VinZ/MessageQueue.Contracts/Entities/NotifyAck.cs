namespace VinZ.MessageQueue;

public record NotifyAck
(
    bool Valid = false,
    HttpStatusCode Status = 0,
    string? data = default,
    ICorrelationId? CorrelationId = default
)
    : INotifyAck
{
    public Task<object?> Response { get; set; } = Task.FromResult<object?>(null);
}