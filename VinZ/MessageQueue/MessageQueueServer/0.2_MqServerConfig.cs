namespace VinZ.MessageQueue;

public record MqServerConfig(
    Type[]? DomainBusTypes = default,

    bool PauseOnError = false,

    int BusRefreshMinMilliseconds = 25,
    int BusRefreshMaxMilliseconds = 500,//7000,

    bool IsTrace = false
);