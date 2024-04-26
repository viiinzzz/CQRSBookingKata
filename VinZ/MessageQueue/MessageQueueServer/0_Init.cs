namespace VinZ.MessageQueue;

public partial class MqServer 
    : Initializable, IMessageBus
{
    private Dictionary<IServiceScope, IMessageBusClient> _domainBuses = new();

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

            var message = $"Time {time.UtcNow.SerializeUniversal()} ({time.state})";

            Notify(new ResponseNotification(default, AuditMessage, message)
            {
                Originator = originator,
                Immediate = true
            });
        };

        if (config.DomainBusTypes == default)
        {
            return;
        }

        foreach (var type in config.DomainBusTypes)
        {
            if (typeof(IMessageBus).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Must implement {nameof(IMessageBus)}", nameof(config.DomainBusTypes));
            }

            var scope = scp.GetScope(type, out var domainBus);

            var client = (IMessageBusClient)domainBus;

            
            client.ConnectToBus(this);

            client.Configure();


            log.Log(LogLevel.Debug,
                $"<<<{type.Name}:{client.GetHashCode().xby4()}>>> Connected.");

            _domainBuses[scope] = client;
        }
    }
}