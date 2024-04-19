namespace VinZ.MessageQueue;

public partial class MessageQueueServer 
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

            Notify(originator, new Notification(default, time.verb)
            {
                Message = message,
                Immediate = true
            });
        };

        if (config.DomainBusType == default)
        {
            return;
        }

        foreach (var type in config.DomainBusType)
        {
            if (typeof(IMessageBus).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Must implement {nameof(IMessageBus)}", nameof(config.DomainBusType));
            }

            var scope = scp.GetScope(type, out var domainBus);

            var client = (IMessageBusClient)domainBus;

            
            client.ConnectToBus(this);

            client.Configure();


            log.LogInformation($"{nameof(MessageQueueServer)}: {type.Name} got connected to main bus.");

            _domainBuses[scope] = client;
        }
    }
}