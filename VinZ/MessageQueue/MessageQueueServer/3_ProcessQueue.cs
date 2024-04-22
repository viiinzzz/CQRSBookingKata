namespace VinZ.MessageQueue;

public partial class MqServer
{
    private async Task<DeliveryCount> ProcessQueue(CancellationToken cancel)
    {
        // log.LogInformation($"Processing message queue...");

        using var scope = scp.GetScope<IMessageQueueRepository>(out var queue);

        var now = DateTime.UtcNow;

        try
        {
            var expired =
                from notification in queue.Notifications
                where now > notification.LatestDelivery &&
                      !notification.Done
                select notification;

            var expiredCount = queue.UpdateNotification(expired, new ServerNotificationUpdate
            {
                Done = true,
                DoneTime = now
            });


            var duplicates =
                from notification in queue.Notifications
                where notification.Aggregate &&
                      !notification.Done
                group notification by new { notification.Recipient, notification.Verb }
                into g
                where g.Count() > 1
                select g.OrderBy(ss => ss.NotificationTime).Skip(1);

            var duplicates2 = new List<ServerNotification>();

            foreach (var dup in duplicates)
            {
                duplicates2.AddRange(dup);
            }

            var duplicateCount = queue.UpdateNotification(duplicates2, new ServerNotificationUpdate
            {
                Done = true,
                DoneTime = now
            });

            var hold =
                from notification in queue.Notifications
                where now < notification.EarliestDelivery &&
                      !notification.Done
                select notification;

            var holdCount = hold.Count();

            queue.ArchiveNotifications();

            if (expiredCount > 0 || duplicateCount > 0 || holdCount > 0)
            {
                var counts = new List<string>();

                if (expiredCount > 0) counts.Add($"expiredCount: {expiredCount}");
                if (duplicateCount > 0) counts.Add($"duplicateCount: {duplicateCount}");
                if (holdCount > 0) counts.Add($"holdCount: {holdCount}");

                log.LogInformation($"Message queue purged. {{{string.Join(", ", counts)}}}");
            }
        }
        catch (Exception ex)
        {
            log.LogError(@$"
Message purge failure: {ex.Message}
{ex.StackTrace}
");
        }


        var messages =
            from notification in queue.Notifications
            where now >= notification.EarliestDelivery &&
                  !notification.Done
            orderby notification.NotificationTime descending
            select notification;

        var count = new DeliveryCount();

        if (!messages.Any())
        {
            return count;
        }

        try
        {
            var updates = messages

                .AsParallel()
                .WithCancellation(cancel)

                .Select(notification => Broadcast(notification, immediate: false, cancel))

                .Aggregate((a, b) =>
                {
                    count = a.Item1 + b.Item1;
                    var updates2 = a.Item2.Concat(b.Item2).ToList();

                    return (count, updates2);
                })
                .Item2;

            foreach (var (notifications, update) in updates)
            {
                queue.UpdateNotification(notifications, update);
            }

        }
        catch (Exception ex)
        {
            log.LogError(@$"
Broadcast failure: {ex.Message}
{ex.StackTrace}
");
        }

        return count;
    }
}