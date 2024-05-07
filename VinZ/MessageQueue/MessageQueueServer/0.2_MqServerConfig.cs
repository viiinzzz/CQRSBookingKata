namespace VinZ.MessageQueue;

public record MqServerConfig
(
    Type[]? DomainBusTypes = default,

    bool PauseOnError = false,

    int BusRefreshMinMilliseconds = 50,
    int BusRefreshMaxMilliseconds = 7000,

    bool IsTrace = false
);