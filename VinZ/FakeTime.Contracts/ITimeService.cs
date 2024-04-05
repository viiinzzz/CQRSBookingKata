namespace VinZ.Common;

public interface ITimeService
{
    ITimeService SetUtcNow(DateTime time);
    ITimeService Freeze();
    ITimeService Unfreeze();
    ITimeService Reset();
    ITimeService Forward(TimeSpan forward);
    DateTime UtcNow { get; }

    event EventHandler<TimeServiceNotification>? Notified;
}