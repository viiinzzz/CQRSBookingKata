
namespace VinZ.MessageQueue;

public record NotifyAck
(
    HttpStatusCode Status = 0,

    bool Valid = false,
    string? data = default,
    CorrelationId? CorrelationId = default
)
   // : INotifyAck
{
    public Task<object?> Response { get; set; } = Task.FromResult<object?>(null);
}
