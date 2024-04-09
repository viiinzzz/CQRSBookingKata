using BookingKata.Services;

namespace BookingKata.Infrastructure.Network;

public class PlanningBus(IScopeProvider sp) : MessageBusClientBase
{
    public override void Configure()
    {
        Subscribe(Recipient.Planning);

        Notified += (sender, notification) =>
        {
            using var scope = sp.GetScope<PlanningQueryService>(out var planning);

            var originator = notification.Originator;
            var correlationGuid = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2).Guid;

            try
            {
                switch (notification.Verb)
                {
                  


                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(originator, ErrorProcessingRequest)
                {
                    CorrelationGuid = correlationGuid,
                    Message = new
                    {
                        message = notification.Message,
                        messageType = notification.MessageType,
                        error = ex.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        };
    }
}