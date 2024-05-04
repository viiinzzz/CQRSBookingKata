﻿namespace VinZ.MessageQueue;

public partial class MqServer
{
    private readonly ConcurrentDictionary<string, AwaitedResponse> _awaiters = new();

    private void Track(AwaitedResponse awaitedResponse)
    {
        if (_awaiters.ContainsKey(awaitedResponse.Key))
        {
            throw new InvalidOperationException("Concurrent wait not allowed");
        }

        _awaiters[awaitedResponse.Key] = awaitedResponse;

        //
        //
        log.LogDebug($"...Track... Correlation={awaitedResponse.Key}");
        //
        //
    }

    private void Untrack(AwaitedResponse awaitedResponse)
    {
        //
        //
        log.LogDebug($"        ...Untrack... Correlation={awaitedResponse.Key}, ElapsedSeconds={awaitedResponse.ElapsedSeconds}, Responded={awaitedResponse.Responded}, Cancelled={awaitedResponse.Cancelled}");
        //
        //

        if (!_awaiters.Remove(awaitedResponse.Key, out _))
        {
            throw new InvalidOperationException("Invalid wait state");
        }
    }




    public async Task<IClientNotificationSerialized?> Wait(NotifyAck ack, CancellationToken cancellationToken)
    {
        var correlationId = ack.CorrelationId;

        if (correlationId == null)
        {
            throw new InvalidOperationException("Uncorrelated wait not allowed");
        }

        var awaitedResponse = new AwaitedResponse(correlationId.Value, DateTime, cancellationToken, Track, Untrack);

        return awaitedResponse?.ResultNotification;
    }

    private IMessageBusClient? GetAwaitedBus(ServerNotification notification)
    {
        if (notification.Type != NotificationType.Response)
        {
            return null;
        }

        var awaiters = _awaiters.Values
            .Where(awaitedResponse => awaitedResponse.IsCorrelatedTo(notification))
            .ToArray();

        var awaiterCount = awaiters.Length;

        var correlationId = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2);
        //
        //
        log.LogDebug($"...Awaiters... Count={awaiterCount}, CorrelationId={correlationId.Guid}, Recipient={notification.Recipient}, Verb={notification.Verb}");
        //
        //

        if (awaiterCount == 0)
        {
            return null;
        }

        return new AwaitedBus(awaiters);
    }

   
}






























/*





   private class AwaitedResponse
   (
       ICorrelationId correlationId, ITimeService DateTime, CancellationToken cancellationToken,
       Action<AwaitedResponse> track, Action<AwaitedResponse> untrack
   )
   {
       public string Key { get; } = correlationId.Guid;

       public bool IsCorrelatedTo(ServerNotification notification)
       {
           return
               notification.CorrelationId1 == correlationId.Id1 &&
               notification.CorrelationId2 == correlationId.Id2;
       }

       private DateTime? StartedTime = DateTime.UtcNow;
       public int ElapsedSeconds { get; private set; } = 0;
       public bool Responded { get; private set; }
       public bool Cancelled => cancellationToken.IsCancellationRequested;
       public IClientNotificationSerialized? ResponseNotification { get; private set; }

       public void Respond(IClientNotificationSerialized notification)
       {
           if (Responded || Cancelled)
           {
               throw new InvalidOperationException();
           }

           Responded = true;
           ResponseNotification = notification;
       }

       private bool resultAlreadyCalled;

       public IClientNotificationSerialized? ResultNotification
       {
           get
           {

               if (resultAlreadyCalled)
               {
                   throw new InvalidOperationException();
               }

               resultAlreadyCalled = true;

               track(this);

               var notification = WaitResponseNotificationAsync().Result;

               untrack(this);

               return notification;

           }
       }

       private async Task<IClientNotificationSerialized?> WaitResponseNotificationAsync()
       {
           while (true)
           {
               await Task.Delay(100);

               if (Responded || Cancelled)
               {
                   break;
               }
           }

           ElapsedSeconds = (
               DateTime.UtcNow - StartedTime.Value
           ).Seconds;

           if (Cancelled || !Responded)
           {
               return null;
           }

           return ResponseNotification;
       }
   }






 private class AwaitedBus(AwaitedResponse[] awaitedResponses) : IMessageBusClient
   {
       public IMessageBusClient ConnectToBus(IMessageBus bus) { return this; }
       public bool Disconnect() { return true; }
       public void/Task Configure() { }
       public void Subscribe(string? recipient, string? verb) { }
       public bool Unsubscribe(string? recipient, string? verb) { return true; }

       public void Notify(IClientNotificationSerialized message) { }

       public void OnNotified(IClientNotificationSerialized notification)
       {
           foreach (var awaitedResponse in awaitedResponses)
           {
               awaitedResponse.Respond(notification);
           }
       }

       public event EventHandler<IClientNotificationSerialized>? Notified;
   }

*/