namespace BookingKata.Infrastructure.Network;

public partial class AdminBus
{
    private void Verb_Is_RequestServerContext(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<IServerContextService>(out var serverContext);

        Notify(new ResponseNotification(Omni, RespondServerContext, serverContext)
        {
            CorrelationId1 = notification.CorrelationId1,
            CorrelationId2 = notification.CorrelationId2
        });
    }
}