namespace VinZ.MessageQueue;

public record MessageQueueServerConfig
(
    Type[]? DomainBusType = default,

    int BusRefreshSeconds = 10, //120
    int BusRefreshMinSeconds = 10
);