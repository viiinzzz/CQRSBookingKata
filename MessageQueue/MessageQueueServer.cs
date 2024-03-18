using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Zejji.Entity;
using Vinz.FakeTime;
using Microsoft.Extensions.Hosting;

namespace Vinz.MessageQueue;


public class MessageQueueServer
    : IMessageQueueServer, IHostedLifecycleService
{
    private readonly ITimeService DateTime;
    private readonly MessageQueueContext _queue;

    public MessageQueueServer(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory>();
         _queue = factory.CreateDbContext<MessageQueueContext>();

        DateTime = scope.ServiceProvider.GetRequiredService<ITimeService>();
    }

    private readonly ConcurrentDictionary<int, IMessageBusClient> _subscribers_0 = new(); //recipient* verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_R = new(); //recipient verb*
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_V = new(); //recipient* verb
    private readonly ConcurrentDictionary<int, HashSet<IMessageBusClient>> _subscribers_RV = new(); //recipient verb


    public void Notify(INotifyMessage notification) => Notify(notification, CancellationToken.None); //fire and forget

    public async Task Notify(INotifyMessage notification, CancellationToken cancel)
    {
        var now = DateTime.UtcNow;

        var n = notification;

        if (n.Message == default && n.Verb == default && n.Recipient == default)
        {
            return;
        }

        var immediate = n.Immediate ?? false;
        var selectEarliest = !immediate && n is { EarliestDelivery: { Ticks: > 0 } };
        var selectLatest = !immediate && n is { LatestDelivery: { Ticks: > 0 } };
        var selectRepeat = n is { RepeatDelay: { Ticks: > 0 }, RepeatCount : > 0 };
        var now2 = immediate && selectRepeat ? now + n.RepeatDelay.Value : now;

        var s = new ServerMessage
        {
            Json = JsonConvert.SerializeObject(n.Message),
            Verb = n.Verb?.ToUpper(),
            Recipient = n.Recipient?.ToUpper(),

            MessageTime = now,
            EarliestDelivery = selectEarliest ? new[] { now2, now + n.EarliestDelivery.Value }.Max() : now,
            LatestDelivery = selectLatest ? now + n.LatestDelivery.Value : System.DateTime.MaxValue,
            RepeatDelay = n.RepeatDelay ?? TimeSpan.Zero,
            RepeatCount = !selectRepeat ? 0 : immediate ? n.RepeatCount.Value - 1 : n.RepeatCount.Value,
            Aggregate = n.Aggregate ?? false,
        };

        if (immediate)
        {
            await Broadcast(s, cancel);
        }

        if (!immediate || s.RepeatCount > 0)
        {
            _queue.Messages.Add(s);

            await _queue.SaveChangesAsync(cancel);
        }
    }


    public int Subscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        if (recipient == default && verb == default)
        {
            var hash0 = client.GetHashCode();

            _subscribers_0[hash0] = client;

            return hash0;
        }

        if (recipient != default && verb == default)
        {
            var hash1 = recipient.ToUpper().GetHashCode();

            _subscribers_R.AddOrUpdate(hash1,
                new HashSet<IMessageBusClient>(new[] { client }),
                (i, subscribers) =>
                {
                    subscribers.Add(client);
                    return subscribers;
                });

            return hash1;
        }

        if (recipient == default && verb != default)
        {
            var hash1 = verb.ToUpper().GetHashCode();

            _subscribers_V.AddOrUpdate(hash1,
                new HashSet<IMessageBusClient>(new[] { client }),
                (i, subscribers) =>
                {
                    subscribers.Add(client);
                    return subscribers;
                });

            return hash1;
        }

        var hash2 = (recipient?.ToUpper(), verb?.ToUpper()).GetHashCode();

        _subscribers_RV.AddOrUpdate(hash2,
            new HashSet<IMessageBusClient>(new[] { client }),
            (i, subscribers) =>
            {
                subscribers.Add(client);
                return subscribers;
            });

        return hash2;
    }

    public bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb)
    {
        if (recipient == default && verb == default)
        {
            return _subscribers_0.Remove(client.GetHashCode(), out _);
        }

        if (recipient != default && verb == default)
        {
            return !_subscribers_R.AddOrUpdate(recipient.ToUpper().GetHashCode(),
                    new HashSet<IMessageBusClient>(),
                    (i, subscribers) =>
                    {
                        subscribers.Remove(client);
                        return subscribers;
                    })
                .Contains(client);
        }

        if (recipient == default && verb != default)
        {
            return !_subscribers_V.AddOrUpdate(verb.ToUpper().GetHashCode(),
                    new HashSet<IMessageBusClient>(),
                    (i, subscribers) =>
                    {
                        subscribers.Remove(client);
                        return subscribers;
                    })
                .Contains(client);
        }

        return !_subscribers_RV.AddOrUpdate((recipient.ToUpper(), verb.ToUpper()).GetHashCode(),
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
        var now = DateTime.UtcNow;


        var expired =
            from s in _queue.Messages
            where now > s.LatestDelivery
            select s;

        await expired.ExecuteDeleteAsync(cancel);


        var duplicate =
            from s in _queue.Messages
            where s.Aggregate
            group s by new { s.Recipient, s.Verb }
            into g
            where g.Count() > 1
            select g.OrderBy(ss => ss.MessageTime).Skip(1);

        await duplicate.ExecuteDeleteAsync(cancel);


        var messages =

            from m in _queue.Messages
            where m.EarliestDelivery > now
            orderby m.MessageTime descending
            select m;

        var count = (await Task.WhenAll(messages
            .AsParallel()
            .WithCancellation(cancel)
            .Select(x => Broadcast(x, cancel))))
            .Aggregate((a, b) => a + b);


        return count;
    }

    private async Task<DeliveryCount> Broadcast(ServerMessage message, CancellationToken cancel)
    {
        var subscribers = new List<IMessageBusClient>();

        if (message.Recipient == default && message.Verb == default)
        {
            var moreSubscribers = _subscribers_0.Values;

            subscribers.AddRange(moreSubscribers);
        }
        else if (message.Recipient != default && message.Verb == default)
        {
            if (_subscribers_R.TryGetValue(message.Recipient.ToUpper().GetHashCode(), out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }
        else if (message.Recipient == default && message.Verb != default)
        {
            if (_subscribers_V.TryGetValue(message.Verb.ToUpper().GetHashCode(), out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }
        else if (message.Recipient != default && message.Verb != default)
        {
            if (_subscribers_RV.TryGetValue((message.Recipient.ToUpper(), message.Verb.ToUpper()).GetHashCode(),
                    out var moreSubscribers))
            {
                subscribers.AddRange(moreSubscribers);
            }
        }

        if (subscribers.Count == 0)
        {
            return new DeliveryCount(0, 0);
        }

        var clientMessage = new ClientMessage
        {
            Message = message.Json == null ? null : JsonConvert.DeserializeObject<dynamic>(message.Json),
            Recipient = message.Recipient?.ToUpper(),
            Verb = message.Verb?.ToUpper(),
        };

        var count = subscribers
            .AsParallel()
            .WithCancellation(_executeCancel.Token)
            .Select((IMessageBusClient client) =>
            {
                try
                {
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


        if (message is { RepeatDelay: { Ticks: > 0 }, RepeatCount: > 1 })
        {
            _queue.Messages.Update(message with
            {
                RepeatCount = message.RepeatCount - 1
            });
        }
        else
        {
            _queue.Messages.Remove(message);
        }

        await _queue.SaveChangesAsync(cancel);

        return count;
    }





    public int BusRefreshSeconds { get; set; } = 120;
 

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
                await Task.Delay(1000 * (BusRefreshSeconds - seconds), cancel);
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