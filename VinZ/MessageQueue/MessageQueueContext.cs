using Microsoft.EntityFrameworkCore;

namespace Vinz.MessageQueue;

public class MessageQueueContext : DbContext
{
    public DbSet<ServerMessage> Messages { get; set; }
}