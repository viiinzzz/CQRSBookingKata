﻿using CQRSBookingKata.Assets;
using CQRSBookingKata.Billing;
using Microsoft.EntityFrameworkCore;

namespace CQRSBookingKata.API.Databases;

public class BookingBackContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder builder) 
        
        => builder.ConfigureMyWay<BookingBackContext>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<Employee>()
            .Property(employee => employee.EmployeeId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Hotel>()
            .Property(hotel => hotel.HotelId)
            .ValueGeneratedOnAdd();

        builder
            .Entity<Hotel>()
            .HasIndex(hotel => hotel.HotelName)
            .IsUnique();

        builder
            .Entity<Room>()
            .HasKey(room => room.Urid);

        builder
            .Entity<Booking>()
            .Property(booking => booking.BookingId)
            .ValueGeneratedOnAdd();
    }
}