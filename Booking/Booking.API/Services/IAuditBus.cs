namespace BookingKata.API.Demo;

public interface IAuditBus
{
    void PrintToConsole(string server, string time, string sender, string recipient, string verb, string message);
}