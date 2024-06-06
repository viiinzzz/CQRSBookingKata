using EntityFrameworkCore.Triggered;
using Microsoft.Extensions.DependencyInjection;

namespace Support.Infrastructure.Storage;

public class ServerNotificationTrigger(MessageQueueContext dbContext) : IAfterSaveTrigger<ServerNotification>
{
    private readonly MqEffects? _effects = dbContext.Effects is MqEffects effects ? effects
        : throw new ArgumentException($"Required type is {nameof(MqEffects)}", nameof(dbContext.Effects));

    public void AfterSave(ITriggerContext<ServerNotification> context)
    {
        var serverNotification = context.Entity;

        if (context.ChangeType == ChangeType.Added)
        {
            _effects.Notify(serverNotification);
        }
    }
}