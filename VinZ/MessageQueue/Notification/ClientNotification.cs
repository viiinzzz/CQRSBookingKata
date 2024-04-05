namespace VinZ.MessageQueue;

public record ClientNotification(
    string? Json = default,
    string? Verb = default,
    string? Recipient = default,
    string? Originator = default,
    long CorrelationId1 = default,
    long CorrelationId2= default
) : IClientNotification;