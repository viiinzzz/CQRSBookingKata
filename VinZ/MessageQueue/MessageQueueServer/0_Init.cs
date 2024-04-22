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
            var message = $"{time.UtcNow.SerializeUniversal()} ({time.state})";

            var originator = GetType().Name;

            Notify(originator, new ResponseNotification(default, time.verb)
            {
                Message = message,
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


            log.LogInformation($"<<<{type.Name}:{client.GetHashCode().xby4()}>>> Connected.");

            _domainBuses[scope] = client;
        }
    }
}