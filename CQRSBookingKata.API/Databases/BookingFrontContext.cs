using CQRSBookingKata.Sales;
using Microsoft.EntityFrameworkCore;

namespace CQRSBookingKata.API.Databases;

public class BookingFrontContext : DbContext
{
    public DbSet<Vacancy> Stock { get; set; }
    public DbSet<StayProposition> Propositions { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlite("Data Source=./BookinkFront.db;");
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
            .Ignore(vacancy => vacancy.Urid);
    }
}