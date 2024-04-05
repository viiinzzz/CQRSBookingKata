namespace BookingKata.API.Infrastructure;

public interface IAuditBus
{
    void Log(string sender, string correlationId, string time, string recipient, string verb, string message);
}