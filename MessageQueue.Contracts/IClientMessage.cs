namespace Vinz.MessageQueue;

public interface IClientMessage
{
    object? Message { get; }
    string? Verb { get; }
    string? Recipient { get; }
}