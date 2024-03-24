namespace Vinz.MessageQueue;

public record ClientMessage(
    object? Message = default,
    string? Verb = default,
    string? Recipient = default
) : IClientMessage;