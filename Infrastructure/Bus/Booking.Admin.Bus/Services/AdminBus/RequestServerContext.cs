namespace BookingKata.Infrastructure.Network;

public partial class AdminBus
{
    private void Verb_Is_RequestServerContext(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<IServerContextService>(out var serverContext);

        Notify(new ResponseNotification(notification, Omni, RespondServerContext, serverContext));
    }
}