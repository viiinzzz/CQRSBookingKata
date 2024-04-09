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

            try
            {
                switch (notification.Verb)
                {
                  


                }
            }
            catch (Exception ex)
            {
                Notify(new NotifyMessage(notification.Originator, ErrorProcessingRequest)
                {
                    CorrelationGuid = notification.CorrelationGuid(),
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