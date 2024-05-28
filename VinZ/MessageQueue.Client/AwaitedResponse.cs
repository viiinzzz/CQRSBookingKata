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

namespace VinZ.MessageQueue;

public class AwaitedResponse
(
    CorrelationId correlationId, ITimeService DateTime, CancellationToken cancellationToken,
    Action<AwaitedResponse> track, Action<AwaitedResponse> untrack
)
{
    private const int CheckMilliseconds = 25;//100;
    public string Key { get; } = correlationId.Guid;

    public bool IsCorrelatedTo(IHaveCorrelation notification)
    {
        return
            notification.CorrelationId1 == correlationId.Id1 &&
            notification.CorrelationId2 == correlationId.Id2;
    }

    private DateTime? StartedTime = DateTime.UtcNow;
    public int ElapsedSeconds { get; private set; } = 0;
    public bool Responded { get; private set; }
    public bool Cancelled => cancellationToken.IsCancellationRequested;
    public IClientNotificationSerialized? ResponseNotification { get; private set; }

    public void Respond(IClientNotificationSerialized notification)
    {
        if (Cancelled)
        {
            throw new InvalidOperationException("respond cancelled not allowed");
        }

        if (/*already*/Responded)
        {
            throw new InvalidOperationException("respond more than once not allowed");
        }

        ResponseNotification = notification;
        Responded = true;
    }

    private bool resultAlreadyCalled;

    public IClientNotificationSerialized? ResultNotification
    {
        get
        {

            if (resultAlreadyCalled)
            {
                throw new InvalidOperationException();
            }

            resultAlreadyCalled = true;

            track(this);

            var notification = WaitResponseNotificationAsync().Result;

            untrack(this);

            return notification;

        }
    }

    private async Task<IClientNotificationSerialized?> WaitResponseNotificationAsync()
    {
        while (true)
        {
            await Task.Delay(CheckMilliseconds);

            if (Cancelled)
            {
                break;
            }
            
            if (Responded)
            {
                break;
            }
        }

        ElapsedSeconds = (
            DateTime.UtcNow - StartedTime.Value
        ).Seconds;

        if (Cancelled)
        {
            return null;
        }
        
        if (!Responded)
        {
            return null;
        }

        return ResponseNotification;
    }
}