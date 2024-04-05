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
        Subscribe(Bus.Any);

        Notified += (sender, notification) =>
        {
            Log(
                $"{sender.GetType().Name}:{server.Id:x16}",
                DateTime.UtcNow.ToString("O"),
                notification.CorrelationId?.ToString("x16") ?? "NULL",
                notification.Recipient,
                notification.Verb,
                notification.Json
            );
        };
    }

    public void Log(string sender, string correlationId, string time, string recipient, string verb, string message)
    {
        log.LogInformation(@$"{{sender:{sender},verb:{verb},recipient:{recipient},correlationId:{correlationId}}}
  message:{message}
");
    }

}