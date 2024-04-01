using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using VinZ.FakeTime;
using VinZ.ToolBox;

namespace VinZ.MessageQueue;

public record MessageQueueServerConfig(Type[]? DomainBusType = default);

public class MessageQueueServer
(
    IScopeProvider sp,
    MessageQueueServerConfig config,
    ITimeService DateTime
)
    : Initializable, IMessageQueueServer, IHostedLifecycleService
{
    private Dictionary<IServiceScope, IMessageBusClient> _domainBuses = new();

    public override void Init()
    {
        DateTime.Notified += (sender, time) =>
        {
            var message = $"{time.UtcNow:s} ({time.state})";

            Notify(new NotifyMessage(default, time.verb)
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

            var scope = sp.GetScope(type, out var domainBus);

            var client = (IMessageBusClient)domainBus;

            client.ConnectTo(this);
            client.Configure();

            Console.Out.WriteLine($"{nameof(MessageQueueServer)}: {type.Name} got connected to main bus.");

            _domainBuses[scope] = client;
        }
    }

    private readonly ConcurrentDictionary<int, IMessageBusClient> _subscribers_0 = new(); //recipient* verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_R = new(); //recipient verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_V = new(); //recipient* verb
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_RV = new(); //recipient verb


    public void Notify(INotifyMessage notifyMessage) => Notify(notifyMessage, CancellationToken.None); //fire and forget

    public async Task Notify(INotifyMessage notifyMessage, CancellationToken cancel)
    {
        var now = DateTime.UtcNow;

        var n = notifyMessage;

        if (n.Message == default && n.Verb == default && n.Recipient == default)
        {
            return;
        }

        var immediate = n.Immediate ?? false;
        var selectEarliest = !immediate && n is { EarliestDelivery: { Ticks: > 0 } };
        var selectLatest = !immediate && n is { LatestDelivery: { Ticks: > 0 } };
        var selectRepeat = n is { RepeatDelay: { Ticks: > 0 }, RepeatCount : > 0 };
        var now2 = immediate && selectRepeat ? now + n.RepeatDelay.Value : now;

        var notification = new ServerNotification
        {
            Json = JsonConvert.SerializeObject(n.Message),
            Verb = n.Verb,
            Recipient = n.Recipient,

            MessageTime = now,
            EarliestDelivery = selectEarliest ? new[] { now2, now + n.EarliestDelivery.Value }.Max() : now,
            LatestDelivery = selectLatest ? now + n.LatestDelivery.Value : System.DateTime.MaxValue,
            RepeatDelay = n.RepeatDelay ?? TimeSpan.Zero,
            RepeatCount = !selectRepeat ? 0 : immediate ? n.RepeatCount.Value - 1 : n.RepeatCount.Value,
            Aggregate = n.Aggregate ?? false,
        };

        if (immediate)
        {
            await Broadcast(notification, true, cancel);
        }

        if (!immediate || notification.RepeatCount > 0)
        {
            using var scope = sp.GetScope<IMessageQueueRepository>(out var queue);


            var queuing = immediate ? "<<<Relaying<<< immediate" : "<<<Queuing<<< scheduled";

            Console.Out.WriteLine($"{queuing} message{(immediate ? "" : "Id:" + notification.MessageId)
            }... {{recipient:{notification.Recipient}, verb:{notification.Verb
            }, message:{notification.Json.Replace("\"", "")}}}");

            queue.AddNotification(notification);
        }
    }


    public int Subscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        var hash0 = client.GetHashCode();

        if (recipient == Bus.Any && verb == Verb.Any)
        {
            _subscribers_0[hash0] = client;

            Console.WriteLine($"[{nameof(MessageQueueServer)}] NEW client {hash0:x8} subscription, recipient=Any, verb=Any, 0+{hash0:x8}");

            return hash0;
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

            Console.WriteLine($"[{nameof(MessageQueueServer)}] NEW client {hash0:x8} subscription, recipient={recipient}, verb=Any, R+{hash1:x8}");

            return hash1;
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

            Console.WriteLine($"[{nameof(MessageQueueServer)}] NEW client {hash0:x8} subscription, recipient=Any, verb={verb}, V+{hash1:x8}");

            return hash1;
        }

        var hash2 = (recipient, verb).GetHashCode();

        _subscribers_RV.AddOrUpdate(hash2,
            new HashSet<IMessageBusClient>(new[] { client }),
            (i, subscribers) =>
            {
                subscribers.Add(client);
                return subscribers;
            });

        Console.WriteLine($"[{nameof(MessageQueueServer)}] ++client {hash0:x8} subscribed, recipient={recipient}, verb={verb}, RV+{hash2:x8}");

        return hash2;
    }

    public bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        var hash0 = client.GetHashCode();

        Console.WriteLine($"[{nameof(MessageQueueServer)}] --client {hash0:x8} unsubscribed, recipient=Any, verb=Any, 0+{hash0:x8}");

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


    private async Task<DeliveryCount> ProcessQueue(CancellationToken cancel)
    {
        Console.Out.WriteLine("Processing message queue...");

        using var scope = sp.GetScope<IMessageQueueRepository>(out var queue);

        var now = DateTime.UtcNow;

        try
        {
            var expired =

                from notification in queue.Notifications

                where now > notification.LatestDelivery &&
                      !notification.Done

                select notification;

            var expiredCount = queue.UpdateNotification(expired, new ServerNotificationUpdate
            {
                Done = true,
                DoneTime = now
            }, scoped: false);


            var duplicates =

                from notification in queue.Notifications

                where notification.Aggregate &&
                      !notification.Done

                group notification by new { notification.Recipient, notification.Verb }

                into g

                where g.Count() > 1

                select g.OrderBy(ss => ss.MessageTime).Skip(1);

            var duplicates2 = new List<ServerNotification>();

            foreach (var dup in duplicates)
            {
                duplicates2.AddRange(dup);
            }

            var duplicateCount = queue.UpdateNotification(duplicates2, new ServerNotificationUpdate
            {
                Done = true,
                DoneTime = now
            }, scoped: false);

            var hold =

                from notification in queue.Notifications

                where now < notification.EarliestDelivery &&
                      !notification.Done

                select notification;

            var holdCount = hold.Count();

            if (expiredCount > 0 || duplicateCount > 0 || holdCount > 0)
            {
                var counts = new List<string>();

                if (expiredCount > 0) counts.Add($"expiredCount: {expiredCount}");
                if (duplicateCount > 0) counts.Add($"duplicateCount: {duplicateCount}");
                if (holdCount > 0) counts.Add($"holdCount: {holdCount}");

                Console.Out.WriteLine($"Message queue purged. {{{string.Join(", ", counts)}}}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(@$"
{nameof(MessageQueueServer)}: purge failure: {ex.Message}
{ex.StackTrace}
");
        }


        var messages =

            from notification in queue.Notifications

            where now >= notification.EarliestDelivery &&
                  !notification.Done

            orderby notification.MessageTime descending

            select notification;

        DeliveryCount count = default;

        if (!messages.Any())
        {
            return count;
        }

        try
        {
            count = (await Task.WhenAll(messages
                    .AsParallel()
                    .WithCancellation(cancel)
                    .Select(x => Broadcast(x, false, cancel))))
                .Aggregate((a, b) => a + b);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(@$"
{nameof(MessageQueueServer)}: broadcast failure: {ex.Message}
{ex.StackTrace}
");
        }

        return count;
    }

    private async Task<DeliveryCount> Broadcast(ServerNotification notification, bool immediate, CancellationToken cancel)
    {
        var subscribers = new List<IMessageBusClient>();

        if (notification.Recipient == default && notification.Verb == default)
        {
            var moreSubscribers = _subscribers_0.Values;

            subscribers.AddRange(moreSubscribers);
        }
        else if (notification.Recipient != default && notification.Verb == default)
        {
            if (_subscribers_R.TryGetValue(notification.Recipient.GetHashCode(), out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }
        else if (notification.Recipient == default && notification.Verb != default)
        {
            if (_subscribers_V.TryGetValue(notification.Verb.GetHashCode(), out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }
        else if (notification.Recipient != default && notification.Verb != default)
        {
            if (_subscribers_RV.TryGetValue((notification.Recipient, notification.Verb).GetHashCode(),
                    out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }

        var dequeuing = immediate ? ">>>Relaying>>> immediate" : ">>>Dequeuing>>> scheduled";

        Console.Out.WriteLine($"{dequeuing} message{(immediate ? "" : "Id:" + notification.MessageId)
        } to {subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")
        }... {{recipient:{notification.Recipient}, verb:{notification.Verb
        }, message:{notification.Json.Replace("\"", "")}}}");

        using var scope = sp.GetScope<IMessageQueueRepository>(out var queue);

        var updateMessage = () =>
        {

            if (notification is { RepeatDelay: { Ticks: > 0 }, RepeatCount: > 1 })
            {
                queue.UpdateNotification(new[] { notification }, new ServerNotificationUpdate
                {
                    RepeatCount = notification.RepeatCount - 1
                }, scoped: false);
            }
            else
            {
                queue.UpdateNotification(new[] { notification }, new ServerNotificationUpdate
                {
                    Done = true,
                    DoneTime = DateTime.UtcNow
                }, scoped: false);
            }
        };

        DeliveryCount count = default;

        if (subscribers.Count == 0)
        {
            if (!immediate) updateMessage();

            return count;
        }

        var clientMessage = new ClientNotification
        {
            Json = notification.Json,
            Recipient = notification.Recipient,
            Verb = notification.Verb,
        };

        count = subscribers
            .AsParallel()
            .WithCancellation(_executeCancel.Token)
            .Select((IMessageBusClient client) =>
            {
                try
                {
                    Console.Out.WriteLine($"{dequeuing}Delivering>>> message {notification.MessageId} to subscriber {client.GetHashCode():x8}...");

                    client.OnNotified(clientMessage);

                    return new DeliveryCount(1, 0);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());

                    return new DeliveryCount(0, 1);
                }
            })
            .Aggregate((a, b) => a + b);

        updateMessage();

        Console.Out.WriteLine($"{dequeuing}Delivering>>>Done message{(immediate ? "" : "Id:" + notification.MessageId)
        } to {subscribers.Count} subscriber{(subscribers.Count > 1 ? "s" : "")
        }... {{recipient:{notification.Recipient}, verb:{notification.Verb
        }, message:{notification.Json.Replace("\"", "")}}}");

        return count;
    }





    public int BusRefreshSeconds { get; set; } = 10; //120
    public int BusRefreshMinSeconds { get; set; } = 10;
 

    private Task _executingTask = Task.CompletedTask;
    private CancellationTokenSource _executeCancel = new();

    
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        //initialize
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _executeCancel = new CancellationTokenSource();

        _executingTask = Execute(_executeCancel.Token);
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        //executing
    }

    private async Task Execute(CancellationToken cancel)
    {
        while (!cancel.IsCancellationRequested)
        {
            var t0 = DateTime.UtcNow;

            //
            //
            await ProcessQueue(cancel);
            //
            //

            var t1 = DateTime.UtcNow;

            var seconds = (int)Math.Ceiling((t1 - t0).TotalSeconds);

            if (seconds < BusRefreshSeconds)
            {
                var delay = 1000 * (BusRefreshSeconds - seconds);

                if (delay < 1000 * BusRefreshMinSeconds)
                {
                    delay = 1000 * BusRefreshMinSeconds;
                }

                await Task.Delay(delay, cancel);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _executeCancel.Cancel();

        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        //tearing down
    }

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        //tear down
    }

}