namespace VinZ.MessageQueue;

public record MessageQueueServerConfig
(
    Type[]? DomainBusType = default,

    int BusRefreshMinMilliseconds = 100,
    int BusRefreshMaxMilliseconds = 15000
);