namespace BookingKata.API.Infrastructure;

public interface IAuditBus
{
    void Audit(object? sender, IClientNotificationSerialized notification);
}
