namespace CQRSBookingKata.API;

public class BookingPlanningContext : DbContext
{
    public DbSet<ReceptionCheck> Checks { get; set; }
    public DbSet<RoomServiceDuty> Duties { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingPlanningContext>();

        builder.EnableSensitiveDataLogging();
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<ReceptionCheck>()
            .HasKey(check => check.CheckId);


        builder
            .Entity<RoomServiceDuty>()
            .HasKey(duty => duty.DutyId);
    }
}