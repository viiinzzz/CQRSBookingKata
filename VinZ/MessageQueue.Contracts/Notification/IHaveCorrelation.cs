namespace VinZ.MessageQueue;

public interface IHaveCorrelation
{
    long CorrelationId1 { get; }
    long CorrelationId2 { get; }
}