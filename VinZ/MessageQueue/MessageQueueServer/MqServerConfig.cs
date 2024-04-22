namespace VinZ.MessageQueue;

public record MqServerConfig
(
    Type[]? DomainBusTypes = default,

    int BusRefreshMinMilliseconds = 100,
    int BusRefreshMaxMilliseconds = 200//15000
);