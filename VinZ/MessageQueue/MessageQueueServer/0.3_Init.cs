namespace VinZ.MessageQueue;


public partial class MqServer : Initializable, IMessageBus
{
    private ConcurrentDictionary<IServiceScope, IMessageBusClient> _domainBuses = new();

    public override void Init()
    {
        // scope = scp.GetScope(out queue);

        DateTime.Notified += (sender, time) =>
        {
            var originator = GetType().Name;

            // var message = new
            // {
            //     // action = time.verb,
            //     time.state,
            //     time = $"{time.UtcNow.SerializeUniversal()}"
            // };

            // var message = $"Time {time.UtcNow.SerializeUniversal()} ({time.state})";
            var message = new
            {
                time = $"{time.UtcNow.SerializeUniversal()}",
                state = $"{time.state}"
            };

            Notify(new ResponseNotification(default, AuditMessage, message)
            {
                Originator = originator,
                Immediate = true
            }, 0);
        };

        if (config.DomainBusTypes == default)
        {
            return;
        }


        var addClient = async (Type type) =>
        {
            if (typeof(IMessageBus).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Must implement {nameof(IMessageBus)}", nameof(config.DomainBusTypes));
            }

            var scope = scp.GetScope(type, out var domainBus);

            var client = (IMessageBusClient)domainBus;

            // client.ConnectToBus(this);
            client.ConnectToBus(scp);

            await client.Configure();

            client.Log = log;

            log.Log(LogLevel.Debug,
                $"<<<{type.Name}:{client.GetHashCode().xby4()}>>> Connected.");

            _domainBuses[scope] = client;
        };


        var allAdded = Task
            .WhenAll(config.DomainBusTypes.Select(type => addClient(type)))
            .ContinueWith(prev =>
            {
                if (prev.IsCompletedSuccessfully)
                {
                    var master = $"<<<{nameof(MqServer)}:{GetHashCode().xby4()}>>>";
                    var clients = (string prepend) => string.Join(Environment.NewLine + prepend, _domainBuses.Values
                        .Select(client => $"<<<{client.GetType().Name}:{client.GetHashCode().xby4()}>>>"));

                    Console.WriteLine(@$"+----------------------------------------
| Bus definition:
| {master}
| {clients("| ")}
+----------------------------------------");
                    log.Log(LogLevel.Debug, @$"Bus definition:
{master}
{clients("")}");
                }
            });


    }
}