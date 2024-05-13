namespace VinZ.MessageQueue;

public record BusConfiguration
(
    string LocalUrl = default,
    string RemoteUrl = default
    // bool IsTrace = false
);