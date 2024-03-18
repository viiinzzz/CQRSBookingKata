﻿namespace Vinz.FakeTime;

public interface ITimeService
{
    ITimeService SetUtcNow(DateTime time);
    ITimeService Freeze();
    ITimeService Unfreeze();
    ITimeService Reset();
    ITimeService Forward(TimeSpan forward);
    DateTime UtcNow { get; }
}