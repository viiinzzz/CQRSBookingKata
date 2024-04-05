namespace Infrastructure.Storage;

public class MessageQueueContext : DbContext
{
    public DbSet<ServerNotification> Notifications { get; set; }
    public DbSet<ServerNotification> ArchivedNotifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<MessageQueueContext>();

        builder.EnableSensitiveDataLogging();
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<ServerNotification>()
            .HasKey(message => message.MessageId);

        builder
            .Entity<ServerNotification>()
            .Property(message => message.MessageId)
            .ValueGeneratedOnAdd();
    }
}