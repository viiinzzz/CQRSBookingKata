namespace VinZ.MessageQueue;

public record ServerNotificationUpdate
(
    int? RepeatCount = default,
    bool? Done = default,
    DateTime? DoneTime = default
);