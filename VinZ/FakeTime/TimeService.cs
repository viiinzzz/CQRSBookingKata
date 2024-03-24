namespace Vinz.FakeTime;

public class TimeService : ITimeService
{
    private TimeSpan? _dt;
    private DateTime? _freeze;

    private void Log(string message) 
        => Console.WriteLine($"{nameof(TimeService)}: [{message}] {UtcNow:s} {(_freeze != default ? "(frozen)" : _dt != default ? "(fake)" : "(real)")}");

    public ITimeService SetUtcNow(DateTime time)
    {
        var utcNow = DateTime.UtcNow;

        _dt = time - utcNow;

        if (_freeze.HasValue) _freeze = utcNow + _dt;

        Log("set");
        return this;
    }

    public ITimeService Freeze()
    {
        _freeze = UtcNow;

        Log("freeze");
        return this;
    }

    public ITimeService Unfreeze()
    {
        _freeze = default;

        Log("unfreeze");
        return this;
    }

    public ITimeService Reset()
    {
        _dt = default;
        _freeze = default;

        Log("reset");
        return this;
    }

    public ITimeService Forward(TimeSpan forward)
    {
        SetUtcNow(UtcNow + forward);

        Log("forward");
        return this;
    }

    private DateTime UtcNow0 => _dt.HasValue ? DateTime.UtcNow + _dt.Value : DateTime.UtcNow;

    public DateTime UtcNow => _freeze ?? UtcNow0;

    public static implicit operator DateTime(TimeService time) => time.UtcNow;
}