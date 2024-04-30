namespace VinZ.MessageQueue;

public interface INotifyAck
{
    ICorrelationId? CorrelationId { get; }
    string? data { get; }
}