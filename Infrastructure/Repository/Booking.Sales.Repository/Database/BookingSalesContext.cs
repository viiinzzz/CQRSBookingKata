namespace BookingKata.Infrastructure.Storage;

public class BookingSalesContext : DbContext
{
    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<StayProposition> Propositions { get; set; }
    public DbSet<Sales.Booking> Bookings { get; set; }


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


        builder
            .Entity<Vacancy>()
            .Ignore(vacancy => vacancy.Position)
            .Ignore(hotel => hotel.geoIndex)
            .Ignore(hotel => hotel.Cells);

        builder
            .Entity<Sales.Booking>()
            .Property(booking => booking.BookingId)
            .ValueGeneratedOnAdd();
    }
}