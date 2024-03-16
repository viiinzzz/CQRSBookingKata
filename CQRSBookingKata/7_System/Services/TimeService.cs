
namespace CQRSBookingKata.ThirdParty;


public class TimeService
{
    private TimeSpan? _dt;
    private DateTime? _freeze;

    public TimeService SetUtcNow(DateTime time)
    {
        var utcNow = DateTime.UtcNow;

        _dt = time - utcNow;

        if (_freeze.HasValue) _freeze = utcNow;

        return this;
    }

    public TimeService Freeze()
    {
        _freeze = UtcNow;

        return this;
    }

    public TimeService Unfreeze()
    {
        _freeze = default;

        return this;
    }

    public TimeService Reset()
    {
        _dt = default;
        _freeze = default;

        return this;
    }

    private DateTime UtcNow0 => _dt.HasValue ? DateTime.UtcNow + _dt.Value : DateTime.UtcNow;

    public DateTime UtcNow => _freeze ?? UtcNow0;

    public static implicit operator DateTime(TimeService time) => time.UtcNow;
}