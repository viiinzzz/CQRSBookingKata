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
        Subscribe(Omni, AuditMessage);

        Notified += Audit;
    }

    private static Regex unquote = new Regex(@"^""(.*)""$");

    public void Audit(object? sender, IClientNotificationSerialized notification)
    {
        var serverLabel = sender == null ? "" : $"<<<Server:{server.Id.xby4()}>>>";
        var senderLabel = sender == null ? "" : $"<<<{sender.GetType().Name}:{sender.GetHashCode().xby4()}>>>";

        var correlation = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2);
        var now = DateTime.UtcNow.ToString("O");

        var messageString = notification.Message.Replace("\\r", "").Replace("\\n", Environment.NewLine);
        messageString = unquote.Replace(Regex.Unescape(messageString), "$1");

        log.Log(LogLevel.Warning, @$"{serverLabel} {senderLabel} Notification{correlation.Guid}
===
{messageString}
===");
    }

}