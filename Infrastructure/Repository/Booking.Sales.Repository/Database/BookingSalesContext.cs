﻿namespace BookingKata.Infrastructure.Storage;

public class BookingSalesContext : MyDbContext
{
    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<StayProposition> Propositions { get; set; }
    public DbSet<Shared.Booking> Bookings { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.ConfigureMyWay<BookingSalesContext>(IsDebug, logLevel);
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
            .Entity<Shared.Booking>()
            .Property(booking => booking.BookingId)
            .ValueGeneratedOnAdd();
    }
}