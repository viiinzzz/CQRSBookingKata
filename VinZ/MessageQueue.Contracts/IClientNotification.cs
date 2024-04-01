namespace VinZ.MessageQueue;

public interface IClientNotification
{
    string? Json { get; }
    string? Verb { get; }
    string? Recipient { get; }
}