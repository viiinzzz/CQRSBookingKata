namespace VinZ.MessageQueue;

public interface ICorrelationId
{
    public long Id1 { get; }
    public long Id2 { get; }
    public string Guid { get; }
}