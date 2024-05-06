namespace VinZ.MessageQueue;

public record ClientRequestNotification() : ClientNotification
(
    NotificationType.Request, 
    default,
    default
);