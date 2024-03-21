
namespace CQRSBookingKata.API;

public class BookingSalesContext : DbContext
{
    public DbSet<Vacancy> Stock { get; set; }
    public DbSet<StayProposition> Propositions { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingSalesContext>();

        builder.EnableSensitiveDataLogging();
    }


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
    }
}