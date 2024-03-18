
namespace CQRSBookingKata.API;

public class BookingFrontContext : DbContext
{
    public DbSet<Vacancy> Stock { get; set; }
    public DbSet<StayProposition> Propositions { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ReceptionCheck> Checks { get; set; }
    public DbSet<RoomServiceDuty> Duties { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)

        => builder.ConfigureMyWay<BookingFrontContext>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Customer>()
            .Property(customer => customer.CustomerId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Customer>()
            .HasIndex(customer => customer.EmailAddress)
            .IsUnique();

        builder
            .Entity<ReceptionCheck>()
            .HasKey(check => check.CheckId);
        builder
            .Entity<RoomServiceDuty>()
            .HasKey(duty => duty.DutyId);
    }
}