namespace VinZ.MessageQueue;

public interface INotifyAck
{
    ICorrelationId? CorrelationId { get; }
}