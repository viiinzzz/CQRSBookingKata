namespace VinZ.MessageQueue;

public partial class MqServer
{
    private readonly ConcurrentDictionary<int, IMessageBusClient> _subscribers_0 = new(); //recipient* verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_R = new(); //recipient verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_V = new(); //recipient* verb
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_RV = new(); //recipient verb
  
    public void Subscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        var hash0 = client.GetHashCode();

        if (recipient == Omni && verb == AnyVerb)
        {
            _subscribers_0[hash0] = client;

            log.LogInformation(
                $"<<<{client.GetType().Name}:{hash0.xby4()}>>> Subscribe recipient={nameof(Omni)}, verb=Any (0+{hash0.xby4()})");

            // hash = hash0;
            return;
        }

        if (recipient != Omni && verb == AnyVerb)
        {
            var hash1 = recipient.GetHashCode();

            _subscribers_R.AddOrUpdate(hash1,
                new HashSet<IMessageBusClient>(new[] { client }),
                (i, subscribers) =>
                {
                    subscribers.Add(client);
                    return subscribers;
                });

            log.LogInformation(
                $"<<<{client.GetType().Name}:{hash0.xby4()}>>> Subscribe recipient={recipient}, verb=Any (R+{hash1.xby4()})");

            // hash = hash1;
            return;
        }

        if (recipient == Omni && verb != AnyVerb)
        {
            var hash1 = verb.GetHashCode();

            _subscribers_V.AddOrUpdate(hash1,
                new HashSet<IMessageBusClient>(new[] { client }),
                (i, subscribers) =>
                {
                    subscribers.Add(client);
                    return subscribers;
                });

            log.LogInformation(
                $"<<<{client.GetType().Name}:{hash0.xby4()}>>> Subscribe recipient={nameof(Omni)}, verb={verb} (V+{hash1.xby4()})");

            // hash = hash1;
            return;
        }

        var hash2 = (recipient, verb).GetHashCode();

        _subscribers_RV.AddOrUpdate(hash2,
            new HashSet<IMessageBusClient>(new[] { client }),
            (i, subscribers) =>
            {
                subscribers.Add(client);
                return subscribers;
            });

        log.LogInformation(
            $"<<<{client.GetType().Name}:{hash0.xby4()}>>> Subscribe recipient={recipient}, verb={verb} (RV+{hash2.xby4()})");

        // hash = hash2;
        return;
    }

    public bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        var hash0 = client.GetHashCode();

        log.LogInformation(
            $"<<<{client.GetType().Name}:{hash0.xby4()}>>> Unsubscribe recipient=Any, verb=Any (0+{hash0.xby4()})");

        if (recipient == Omni && verb == AnyVerb)
        {
            return _subscribers_0.Remove(client.GetHashCode(), out _);
        }

        if (recipient != Omni && verb == AnyVerb)
        {
            return !_subscribers_R.AddOrUpdate(recipient.GetHashCode(),
                    new HashSet<IMessageBusClient>(),
                    (i, subscribers) =>
                    {
                        subscribers.Remove(client);
                        return subscribers;
                    })
                .Contains(client);
        }

        if (recipient == Omni && verb != AnyVerb)
        {
            return !_subscribers_V.AddOrUpdate(verb.GetHashCode(),
                    new HashSet<IMessageBusClient>(),
                    (i, subscribers) =>
                    {
                        subscribers.Remove(client);
                        return subscribers;
                    })
                .Contains(client);
        }

        return !_subscribers_RV.AddOrUpdate((recipient, verb).GetHashCode(),
                new HashSet<IMessageBusClient>(),
                (i, subscribers) =>
                {
                    subscribers.Remove(client);
                    return subscribers;
                })
            .Contains(client);
    }
}