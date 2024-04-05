using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace VinZ.MessageQueue;

public partial class MessageQueueServer
{
    private readonly ConcurrentDictionary<int, IMessageBusClient> _subscribers_0 = new(); //recipient* verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_R = new(); //recipient verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_V = new(); //recipient* verb
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_RV = new(); //recipient verb
  
    public void Subscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        var hash0 = client.GetHashCode();

        if (recipient == Bus.Any && verb == Verb.Any)
        {
            _subscribers_0[hash0] = client;

            log.LogInformation(
                $"[{nameof(MessageQueueServer)}] NEW client {hash0:x8} subscription, recipient=Any, verb=Any, 0+{hash0:x8}");

            // hash = hash0;
            return;
        }

        if (recipient != Bus.Any && verb == Verb.Any)
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
                $"[{nameof(MessageQueueServer)}] NEW client {hash0:x8} subscription, recipient={recipient}, verb=Any, R+{hash1:x8}");

            // hash = hash1;
            return;
        }

        if (recipient == Bus.Any && verb != Verb.Any)
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
                $"[{nameof(MessageQueueServer)}] NEW client {hash0:x8} subscription, recipient=Any, verb={verb}, V+{hash1:x8}");

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
            $"[{nameof(MessageQueueServer)}] ++client {hash0:x8} subscribed, recipient={recipient}, verb={verb}, RV+{hash2:x8}");

        // hash = hash2;
        return;
    }

    public bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        var hash0 = client.GetHashCode();

        log.LogInformation(
            $"[{nameof(MessageQueueServer)}] --client {hash0:x8} unsubscribed, recipient=Any, verb=Any, 0+{hash0:x8}");

        if (recipient == Bus.Any && verb == Verb.Any)
        {
            return _subscribers_0.Remove(client.GetHashCode(), out _);
        }

        if (recipient != Bus.Any && verb == Verb.Any)
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

        if (recipient == Bus.Any && verb != Verb.Any)
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