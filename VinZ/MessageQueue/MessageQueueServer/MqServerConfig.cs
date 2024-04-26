namespace VinZ.MessageQueue;

public record MqServerConfig
(
    Type[]? DomainBusTypes = default,

    int BusRefreshMinMilliseconds = 50,
    int BusRefreshMaxMilliseconds = 7000
);