/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace VinZ.Common;

public class TimeService : ITimeService
{
    private TimeSpan? _dt;
    private DateTime? _freeze;


    public event EventHandler<TimeServiceNotification>? Notified;

    public virtual void OnNotified(TimeServiceNotification notification)
    {
        Notified?.Invoke(this, notification);
    }


    private void Notify(string verb)
    {
        var utcNow = UtcNow;

        var freeze = _freeze != default;
        var fake = _dt != default;
        var state = freeze ? "frozen" : fake ? "fake" : "real";
        
        var notification = new TimeServiceNotification(verb, utcNow, freeze, fake, state);
        
        OnNotified(notification);
    }

    public ITimeService SetUtcNow(DateTime time)
    {
        var utcNow = DateTime.UtcNow;

        _dt = time - utcNow;

        if (_freeze.HasValue) _freeze = utcNow + _dt;

        Notify(TimeServiceConst.Verb.Set);
        return this;
    }

    public ITimeService Freeze()
    {
        _freeze = UtcNow;

        Notify(TimeServiceConst.Verb.Freeze);
        return this;
    }

    public ITimeService Unfreeze()
    {
        _freeze = default;

        Notify(TimeServiceConst.Verb.Unfreeze);
        return this;
    }

    public ITimeService Reset()
    {
        _dt = default;
        _freeze = default;

        Notify(TimeServiceConst.Verb.Reset);
        return this;
    }

    public ITimeService Forward(TimeSpan forward)
    {
        SetUtcNow(UtcNow + forward);

        Notify(TimeServiceConst.Verb.Forward);
        return this;
    }

    private DateTime UtcNow0 => _dt.HasValue ? DateTime.UtcNow + _dt.Value : DateTime.UtcNow;

    public DateTime UtcNow => _freeze ?? UtcNow0;

    public static implicit operator DateTime(TimeService time) => time.UtcNow;
}