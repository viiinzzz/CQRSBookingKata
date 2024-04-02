using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BookingKata.API;

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

    public int UpdateNotification(IEnumerable<ServerNotification> notifications, ServerNotificationUpdate update, bool scoped)
    {
        var count = 0;

        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var updated = new List<EntityEntry<ServerNotification>>();

            foreach (var notification in notifications)
            {
                var notification2 = _queue.Notifications
                    .Find(notification.MessageId);

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

            foreach(var entity in updated)
            {
                entity.State = EntityState.Detached;
            }

            scope?.Complete();

            return count;
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

}