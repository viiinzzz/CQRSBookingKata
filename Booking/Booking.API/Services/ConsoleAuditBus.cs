using Newtonsoft.Json;

namespace BookingKata.API.Demo;


public class ConsoleAuditBus
(
    ITimeService DateTime,
    IServerContextService server
)
    : MessageBusClientBase, IAuditBus
{
    public override void Configure()
    {
        Subscribe(default, default);

        Notified += (sender, message) =>
        {
            PrintToConsole(
                $"{sender.GetType().Name}:{server.Id:x16}",
                DateTime.UtcNow.ToString("O"),
                sender.ToString(),
                message.Recipient, 
                message.Verb,
                message.Json
            );
        };
    }

    public void PrintToConsole(string server, string time, string sender, string recipient, string verb, string message)
    {
        Console.WriteLine(@$"BUS: sender={sender} sent verb '{verb}' to recipient={recipient}
     message={message}
");
    }

}