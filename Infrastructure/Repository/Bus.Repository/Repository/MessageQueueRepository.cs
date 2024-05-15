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

namespace Support.Infrastructure.Storage;

public class MessageQueueRepository
(
    IDbContextFactory factory,
    ITimeService DateTime
)
    : IMessageQueueRepository, ITransactionable
{
    private readonly MessageQueueContext _queue = factory.CreateDbContext<MessageQueueContext>();

    public TransactionContext AsTransaction() => new TransactionContext() * _queue;

    public IQueryable<ServerNotification> Notifications

        => _queue.Notifications
            .AsNoTracking();

    public void AddNotification(ServerNotification notification)
    {
        var entity = _queue.Notifications.Add(notification);
        _queue.SaveChanges();
        entity.State = EntityState.Detached;

        _queue.SaveChanges();
    }


    public int ArchiveNotifications()
    {
        var done =

            from notification in _queue.Notifications

            where notification.Done

            select notification;

        _queue.ArchivedNotifications.AddRange(done);

        return done.ExecuteDelete();
    }

    public int UpdateNotification(IEnumerable<ServerNotification> notifications, ServerNotificationUpdate update)
    {
        var count = 0;

        var updated = new List<EntityEntry<ServerNotification>>();

        foreach (var notification in notifications)
        {
            var notification2 = _queue.Notifications
                .Find(notification.NotificationId);

            if (notification2 == default)
            {
                throw new InvalidOperationException("messageId not found");
            }

            _queue.Entry(notification2).State = EntityState.Detached;

            if (update.RepeatCount.HasValue)
            {
                notification2 = notification2 with
                {
                    RepeatCount = update.RepeatCount.Value
                };
            }

            if (update.Done.HasValue)
            {
                notification2 = notification2 with
                {
                    Done = update.Done.Value
                };
            }

            if (update.DoneTime.HasValue)
            {
                notification2 = notification2 with
                {
                    DoneTime = update.DoneTime.Value
                };
            }

            var entity = _queue.Notifications.Update(notification2);

            updated.Add(entity);

            count++;
        }

        _queue.SaveChanges();

        foreach (var entity in updated)
        {
            entity.State = EntityState.Detached;
        }

        return count;
    }

}