namespace VinZ.MessageQueue;

public record ClientNotification(
    string? Json = default,
    string? Verb = default,
    string? Recipient = default
) : IClientNotification;