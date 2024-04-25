namespace BookingKata.API.Infrastructure;


public class ConsoleAuditBus
(
    ITimeService DateTime,
    IServerContextService server,
    ILogger<ConsoleAuditBus> log
)
    : MessageBusClientBase, IAuditBus
{
    public override void Configure()
    {
        Subscribe(Omni, InformationMessage);

        Notified += (sender, notification) =>
        {
            Log(
                DateTime.UtcNow.ToString("O"),
                new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid,

                sender == null ? null : $"{sender.GetType().Name}:{server.Id.xby4()}",
                notification.Recipient,
                notification.Verb ,

                notification.Message
            );
        };
    }

    public void Log
    (
        string correlationId,
        string time,
        
        string? sender,
        string? recipient,
        string? verb,
        
        string? message
    )
    {
        log.LogInformation(@$"{{sender:{sender},verb:{verb},recipient:{recipient},correlationId:{correlationId}}}
  message:{message}
");
    }

}