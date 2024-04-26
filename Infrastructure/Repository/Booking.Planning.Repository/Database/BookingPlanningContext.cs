namespace BookingKata.Infrastructure.Storage;

public class BookingPlanningContext : MyDbContext
{
    public DbSet<ReceptionCheck> Checks { get; set; }
    public DbSet<RoomServiceDuty> Duties { get; set; }
    public DbSet<ServerContext> ServerContext { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingPlanningContext>(IsDebug, IsTrace);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {

        builder
            .Entity<ReceptionCheck>()
            .HasKey(check => check.CheckId);


        builder
            .Entity<RoomServiceDuty>()
            .HasKey(duty => duty.DutyId);

        // builder
        //     .Entity<RoomServiceDuty>()
        //     .HasOne<ServerContext>(duty => duty.Server);


        builder
            .Entity<ServerContext>()
            .HasKey(serverContext => serverContext.ServerContextId);
    }
}