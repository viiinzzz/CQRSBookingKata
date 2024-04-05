namespace VinZ.MessageQueue;

public record NotifyAck
(
    bool Valid = false,
    ICorrelationId? CorrelationId = default
) : INotifyAck;