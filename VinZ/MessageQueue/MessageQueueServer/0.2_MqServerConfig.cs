namespace VinZ.MessageQueue;

public record MqServerConfig
(
    Type[]? DomainBusTypes = default,

    bool PauseOnError = false,

    int BusRefreshMinMilliseconds = 5,//50
    int BusRefreshMaxMilliseconds = 5,//7000,

    bool IsTrace = false
);