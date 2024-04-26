namespace Infrastructure.Storage;

public class MessageQueueContext : MyDbContext
{
    public DbSet<ServerNotification> Notifications { get; set; }
    public DbSet<ServerNotification> ArchivedNotifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<MessageQueueContext>(IsDebug, IsTrace);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<ServerNotification>()
            .HasKey(message => message.NotificationId);

        builder
            .Entity<ServerNotification>()
            .Property(message => message.NotificationId)
            .ValueGeneratedOnAdd();
    }
}