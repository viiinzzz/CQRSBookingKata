namespace VinZ.MessageQueue;

public interface IClientNotification
{
    string? Json { get; }
    string? Verb { get; }
    string? Recipient { get; }
    string? Originator { get; }
    long CorrelationId1 { get; }
    long CorrelationId2 { get; }
}