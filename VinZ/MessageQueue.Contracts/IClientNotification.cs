namespace VinZ.MessageQueue;

public interface IClientNotification
{
    string? Message { get; }
    string? MessageType { get; }
    TMessage MessageAs<TMessage>();
    string? Verb { get; }
    string? Recipient { get; }
    string? Originator { get; }
    long CorrelationId1 { get; }
    long CorrelationId2 { get; }
}